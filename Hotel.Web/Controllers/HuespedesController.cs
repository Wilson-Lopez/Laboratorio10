using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class HuespedesController : Controller
{
    private readonly HuespedRepositorio _repositorio;

    public HuespedesController(HuespedRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Huespedes?buscar=torres
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;
        return View(await _repositorio.ListarActivosAsync(buscar));
    }

    // GET: Huespedes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var huesped = await _repositorio.ObtenerConReservasAsync(id);
        if (huesped == null)
            return NotFound();

        return View(huesped);
    }

    // GET: Huespedes/Create
    // Si viene desde "Nueva reserva" (paraReserva = true), al guardar vuelve a la reserva con el huésped seleccionado.
    public IActionResult Create(bool paraReserva = false, int? idHabitacion = null, DateOnly? ingreso = null, DateOnly? salida = null)
    {
        GuardarContextoReserva(paraReserva, idHabitacion, ingreso, salida);
        return View(new Huesped());
    }

    // POST: Huespedes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombres,Apellidos,Documento,Email,Telefono")] Huesped huesped,
        bool paraReserva = false, int? idHabitacion = null, DateOnly? ingreso = null, DateOnly? salida = null)
    {
        Normalizar(huesped);
        await ValidarDocumentoAsync(huesped, null);

        if (!ModelState.IsValid)
        {
            GuardarContextoReserva(paraReserva, idHabitacion, ingreso, salida);
            return View(huesped);
        }

        await _repositorio.CrearAsync(huesped);

        if (paraReserva)
        {
            TempData["Mensaje"] = $"Huésped {huesped.NombreCompleto} registrado. Complete los datos de la reserva.";
            return RedirectToAction("Create", "Reservas",
                new
                {
                    idHuesped = huesped.IdHuesped,
                    idHabitacion,
                    ingreso = ingreso?.ToString("yyyy-MM-dd"),
                    salida = salida?.ToString("yyyy-MM-dd")
                });
        }

        TempData["Mensaje"] = $"Huésped {huesped.NombreCompleto} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Huespedes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var huesped = await _repositorio.ObtenerActivoAsync(id);
        if (huesped == null)
            return NotFound();

        return View(huesped);
    }

    // POST: Huespedes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHuesped,Nombres,Apellidos,Documento,Email,Telefono")] Huesped huesped)
    {
        if (id != huesped.IdHuesped)
            return NotFound();

        Normalizar(huesped);
        await ValidarDocumentoAsync(huesped, huesped.IdHuesped);

        if (!ModelState.IsValid)
            return View(huesped);

        if (!await _repositorio.ActualizarAsync(huesped))
            return NotFound();

        TempData["Mensaje"] = $"Huésped {huesped.NombreCompleto} actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Huespedes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var huesped = await _repositorio.ObtenerActivoAsync(id);
        if (huesped == null)
            return NotFound();

        ViewBag.TieneReservasVigentes = await _repositorio.TieneReservasVigentesAsync(id);
        return View(huesped);
    }

    // POST: Huespedes/Delete/5 (eliminación lógica)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _repositorio.TieneReservasVigentesAsync(id))
        {
            TempData["Error"] = "No se puede eliminar: el huésped tiene reservas pendientes o confirmadas. Cancélelas primero.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _repositorio.EliminarAsync(id))
            return NotFound();

        TempData["Mensaje"] = "Huésped eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalizar(Huesped huesped)
    {
        huesped.Nombres = huesped.Nombres?.Trim()!;
        huesped.Apellidos = huesped.Apellidos?.Trim()!;
        huesped.Documento = huesped.Documento?.Trim().ToUpperInvariant()!;
        huesped.Email = string.IsNullOrWhiteSpace(huesped.Email) ? null : huesped.Email.Trim().ToLowerInvariant();
        huesped.Telefono = string.IsNullOrWhiteSpace(huesped.Telefono) ? null : huesped.Telefono.Trim();
    }

    private async Task ValidarDocumentoAsync(Huesped huesped, int? excluirId)
    {
        if (string.IsNullOrWhiteSpace(huesped.Documento))
            return;

        var existente = await _repositorio.BuscarPorDocumentoAsync(huesped.Documento, excluirId);
        if (existente == null)
            return;

        ModelState.AddModelError(nameof(Huesped.Documento), existente.Activo
            ? $"El documento {huesped.Documento} ya pertenece a {existente.NombreCompleto}."
            : $"El documento {huesped.Documento} pertenece a un huésped dado de baja.");
    }

    private void GuardarContextoReserva(bool paraReserva, int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.ParaReserva = paraReserva;
        ViewBag.IdHabitacion = idHabitacion;
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;
    }
}
