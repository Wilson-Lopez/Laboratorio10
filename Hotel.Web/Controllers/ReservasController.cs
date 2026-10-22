using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repositorio;

    public ReservasController(ReservaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    // GET: Reservas → muestra directamente el formulario de nueva reserva
    public Task<IActionResult> Index() => Create(null, null, null, null);

    // GET: Reservas/Listado?estado=Pendiente&buscar=torres (reservas registradas, con confirmar/finalizar/cancelar)
    public async Task<IActionResult> Listado(string? estado, string? buscar)
    {
        ViewBag.Estado = new SelectList(EstadoReserva.Todos, estado);
        ViewBag.Buscar = buscar;
        return View(await _repositorio.ListarAsync(estado, buscar));
    }

    // GET: Reservas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var reserva = await _repositorio.ObtenerAsync(id);
        if (reserva == null)
            return NotFound();

        return View(reserva);
    }

    // GET: Reservas/Disponibles?ingreso=2026-10-22&salida=2026-10-25&capacidad=2&idTipo=3
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida, int? capacidad, int? idTipo)
    {
        await CargarBuscadorAsync(ingreso, salida, capacidad, idTipo);

        if (!ingreso.HasValue || !salida.HasValue)
        {
            ViewBag.NoDisponibles = new List<Habitacion>();
            return View(new List<Habitacion>());
        }

        return View(await BuscarDisponibilidadAsync(ingreso.Value, salida.Value, capacidad, idTipo));
    }

    // Valida el rango y devuelve las habitaciones libres; deja en ViewBag las noches y las no disponibles
    private async Task<List<Habitacion>> BuscarDisponibilidadAsync(DateOnly ingreso, DateOnly salida, int? capacidad, int? idTipo)
    {
        ViewBag.NoDisponibles = new List<Habitacion>();

        if (ingreso < Hoy)
            ViewBag.Error = "La fecha de entrada no puede ser anterior a hoy.";
        else if (salida <= ingreso)
            ViewBag.Error = "La fecha de salida debe ser posterior a la de entrada.";

        if (ViewBag.Error != null)
            return new List<Habitacion>();

        ViewBag.Noches = salida.DayNumber - ingreso.DayNumber;
        ViewBag.NoDisponibles = await _repositorio.ListarNoDisponiblesAsync(ingreso, salida, capacidad, idTipo);
        return await _repositorio.ListarDisponiblesAsync(ingreso, salida, capacidad, idTipo);
    }

    // GET: Reservas/Create (opcionalmente precargado desde Disponibles o desde el registro de un huésped)
    public async Task<IActionResult> Create(int? idHuesped, int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        var reserva = new Reserva
        {
            IdHuesped = idHuesped ?? 0,
            IdHabitacion = idHabitacion ?? 0,
            FechaIngreso = ingreso ?? Hoy,
            FechaSalida = salida ?? (ingreso ?? Hoy).AddDays(1)
        };

        await CargarListasAsync(reserva);
        return View(nameof(Create), reserva);
    }

    // POST: Reservas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida,Observaciones")] Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.Estado));
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));

        reserva.Observaciones = string.IsNullOrWhiteSpace(reserva.Observaciones) ? null : reserva.Observaciones.Trim();

        var fechasValidas = true;
        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la de ingreso.");
            fechasValidas = false;
        }

        if (reserva.FechaIngreso < Hoy)
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
            fechasValidas = false;
        }

        if (reserva.IdHuesped > 0 && !await _repositorio.HuespedActivoAsync(reserva.IdHuesped))
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "El huésped seleccionado no existe o fue dado de baja.");

        var habitacionValida = reserva.IdHabitacion > 0 && await _repositorio.HabitacionActivaAsync(reserva.IdHabitacion);
        if (reserva.IdHabitacion > 0 && !habitacionValida)
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación seleccionada no existe o fue dada de baja.");

        if (fechasValidas && habitacionValida &&
            await _repositorio.HabitacionReservadaAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango de fechas.");
        }

        if (ModelState.IsValid)
        {
            if (await _repositorio.CrearAsync(reserva))
            {
                TempData["Mensaje"] = $"Reserva #{reserva.IdReserva} registrada con estado Pendiente.";
                return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
            }

            // Otro usuario reservó la habitación entre la validación y el guardado
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación acaba de ser reservada en ese rango de fechas.");
        }

        await CargarListasAsync(reserva);
        return View(reserva);
    }

    // POST: Reservas/Confirmar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Confirmar(int id, bool volverADetalle = false) =>
        CambiarEstadoAsync(id, EstadoReserva.Confirmada, "confirmada", volverADetalle);

    // POST: Reservas/Finalizar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Finalizar(int id, bool volverADetalle = false) =>
        CambiarEstadoAsync(id, EstadoReserva.Finalizada, "finalizada", volverADetalle);

    // POST: Reservas/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cancelar(int id, bool volverADetalle = false) =>
        CambiarEstadoAsync(id, EstadoReserva.Cancelada, "cancelada", volverADetalle);

    private async Task<IActionResult> CambiarEstadoAsync(int id, string nuevoEstado, string verbo, bool volverADetalle)
    {
        if (await _repositorio.CambiarEstadoAsync(id, nuevoEstado))
            TempData["Mensaje"] = $"Reserva #{id} {verbo} correctamente.";
        else
            TempData["Error"] = $"La reserva #{id} no existe o su estado actual no permite que sea {verbo}.";

        return volverADetalle
            ? RedirectToAction(nameof(Details), new { id })
            : RedirectToAction(nameof(Listado));
    }

    // Datos del buscador de disponibilidad (parcial _BuscadorReserva)
    private async Task CargarBuscadorAsync(DateOnly? ingreso, DateOnly? salida, int? capacidad, int? idTipo)
    {
        var tipos = await _repositorio.ListarTiposAsync();
        var capacidadMaxima = tipos.Count > 0 ? tipos.Max(t => t.Capacidad) : 1;

        ViewBag.BuscadorIngreso = ingreso ?? Hoy;
        ViewBag.BuscadorSalida = salida ?? (ingreso ?? Hoy).AddDays(1);
        ViewBag.BuscadorTipos = new SelectList(tipos, "IdTipo", "Nombre", idTipo);
        ViewBag.BuscadorCapacidades = new SelectList(
            Enumerable.Range(1, capacidadMaxima)
                      .Select(c => new { Valor = c, Texto = c == 1 ? "1 huésped" : $"{c} huéspedes" }),
            "Valor", "Texto", capacidad);
    }

    private async Task CargarListasAsync(Reserva reserva)
    {
        var huespedes = await _repositorio.ListarHuespedesActivosAsync();
        var habitaciones = await _repositorio.ListarHabitacionesActivasAsync();

        ViewBag.IdHuesped = new SelectList(
            huespedes.Select(h => new { h.IdHuesped, Texto = $"{h.NombreCompleto} — {h.Documento}" }),
            "IdHuesped", "Texto", reserva.IdHuesped);

        ViewBag.IdHabitacion = new SelectList(
            habitaciones.Select(h => new
            {
                h.IdHabitacion,
                Texto = $"{h.Numero} — {h.IdTipoNavigation.Nombre}, {h.IdTipoNavigation.Capacidad} pers. (S/ {h.IdTipoNavigation.PrecioNoche:N2} por noche)"
            }),
            "IdHabitacion", "Texto", reserva.IdHabitacion);

        // Para calcular noches y total estimado en el formulario
        ViewBag.Precios = habitaciones.ToDictionary(h => h.IdHabitacion, h => h.IdTipoNavigation.PrecioNoche);
    }
}
