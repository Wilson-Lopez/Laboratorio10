using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HuespedRepositorio
{
    private readonly HotelContext _context;

    public HuespedRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Huéspedes activos, búsqueda opcional por nombre, apellido o documento
    public async Task<List<Huesped>> ListarActivosAsync(string? buscar)
    {
        var consulta = _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(h => h.Nombres.Contains(buscar) ||
                                           h.Apellidos.Contains(buscar) ||
                                           h.Documento.Contains(buscar));
        }

        return await consulta
            .OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<Huesped?> ObtenerActivoAsync(int id)
    {
        return await _context.Huespeds
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdHuesped == id && h.Activo);
    }

    // Huésped con su historial de reservas (más recientes primero)
    public async Task<Huesped?> ObtenerConReservasAsync(int id)
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Include(h => h.Reservas.OrderByDescending(r => r.FechaIngreso))
                .ThenInclude(r => r.IdHabitacionNavigation)
                    .ThenInclude(hab => hab.IdTipoNavigation)
            .FirstOrDefaultAsync(h => h.IdHuesped == id && h.Activo);
    }

    // Incluye huéspedes inactivos porque Documento es UNIQUE en la tabla
    public async Task<Huesped?> BuscarPorDocumentoAsync(string documento, int? excluirId = null)
    {
        return await _context.Huespeds
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Documento == documento && h.IdHuesped != (excluirId ?? 0));
    }

    public async Task<bool> TieneReservasVigentesAsync(int id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.IdHuesped == id &&
                           (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada) &&
                           r.FechaSalida > hoy);
    }

    public async Task CrearAsync(Huesped huesped)
    {
        huesped.Activo = true;
        _context.Huespeds.Add(huesped);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Huesped huesped)
    {
        var actual = await _context.Huespeds
            .FirstOrDefaultAsync(h => h.IdHuesped == huesped.IdHuesped && h.Activo);
        if (actual == null)
            return false;

        actual.Nombres = huesped.Nombres;
        actual.Apellidos = huesped.Apellidos;
        actual.Documento = huesped.Documento;
        actual.Email = huesped.Email;
        actual.Telefono = huesped.Telefono;
        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminación lógica: Activo = 0
    public async Task<bool> EliminarAsync(int id)
    {
        var huesped = await _context.Huespeds
            .FirstOrDefaultAsync(h => h.IdHuesped == id && h.Activo);
        if (huesped == null)
            return false;

        huesped.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
