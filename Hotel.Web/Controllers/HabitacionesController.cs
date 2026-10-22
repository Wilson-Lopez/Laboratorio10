using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repositorio;

    public HabitacionesController(HabitacionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Habitaciones?idTipo=2&capacidad=2&precioMax=250
    public async Task<IActionResult> Index(int? idTipo, int? capacidad, decimal? precioMax)
    {
        var tipos = await _repositorio.ListarTiposAsync();
        var precioTope = tipos.Count > 0 ? tipos.Max(t => t.PrecioNoche) : 0;

        // El tope del rango equivale a "sin límite"
        if (precioMax >= precioTope)
            precioMax = null;

        ViewBag.IdTipo = new SelectList(tipos, "IdTipo", "Nombre", idTipo);
        ViewBag.Capacidad = new SelectList(
            tipos.Select(t => t.Capacidad).Distinct().Order()
                 .Select(c => new { Valor = c, Texto = c == 1 ? "1 huésped o más" : $"{c} huéspedes o más" }),
            "Valor", "Texto", capacidad);
        ViewBag.PrecioMin = tipos.Count > 0 ? tipos.Min(t => t.PrecioNoche) : 0;
        ViewBag.PrecioTope = precioTope;
        ViewBag.PrecioMax = precioMax;
        ViewBag.HayFiltros = idTipo.HasValue || capacidad.HasValue || precioMax.HasValue;
        ViewBag.Ocupadas = (await _repositorio.ListarIdsOcupadasAsync(DateOnly.FromDateTime(DateTime.Today))).ToHashSet();

        return View(await _repositorio.ListarActivasAsync(idTipo, capacidad, precioMax));
    }

    // GET: Habitaciones/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await _repositorio.ObtenerConReservasVigentesAsync(id);
        if (habitacion == null)
            return NotFound();

        return View(habitacion);
    }

    // GET: Habitaciones/Create
    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync(null);
        return View(new Habitacion { Piso = 1 });
    }

    // POST: Habitaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipo")] Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoNavigation));
        habitacion.Numero = habitacion.Numero?.Trim().ToUpperInvariant()!;

        if (!string.IsNullOrWhiteSpace(habitacion.Numero) &&
            await _repositorio.ExisteNumeroAsync(habitacion.Numero))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero),
                $"Ya existe una habitación con el número {habitacion.Numero}.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipo);
            return View(habitacion);
        }

        await _repositorio.CrearAsync(habitacion);
        TempData["Mensaje"] = $"Habitación {habitacion.Numero} registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Habitaciones/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _repositorio.ObtenerActivaAsync(id);
        if (habitacion == null)
            return NotFound();

        await CargarTiposAsync(habitacion.IdTipo);
        return View(habitacion);
    }

    // POST: Habitaciones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipo")] Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion)
            return NotFound();

        ModelState.Remove(nameof(Habitacion.IdTipoNavigation));
        habitacion.Numero = habitacion.Numero?.Trim().ToUpperInvariant()!;

        if (!string.IsNullOrWhiteSpace(habitacion.Numero) &&
            await _repositorio.ExisteNumeroAsync(habitacion.Numero, habitacion.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero),
                $"Ya existe otra habitación con el número {habitacion.Numero}.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipo);
            return View(habitacion);
        }

        if (!await _repositorio.ActualizarAsync(habitacion))
            return NotFound();

        TempData["Mensaje"] = $"Habitación {habitacion.Numero} actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Habitaciones/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await _repositorio.ObtenerActivaAsync(id);
        if (habitacion == null)
            return NotFound();

        ViewBag.TieneReservasVigentes = await _repositorio.TieneReservasVigentesAsync(id);
        return View(habitacion);
    }

    // POST: Habitaciones/Delete/5 (eliminación lógica)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _repositorio.TieneReservasVigentesAsync(id))
        {
            TempData["Error"] = "No se puede eliminar: la habitación tiene reservas pendientes o confirmadas. Cancélelas primero.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _repositorio.EliminarAsync(id))
            return NotFound();

        TempData["Mensaje"] = "Habitación eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? seleccionado)
    {
        var tipos = await _repositorio.ListarTiposAsync();
        ViewBag.IdTipo = new SelectList(tipos, "IdTipo", "Nombre", seleccionado);
    }
}
