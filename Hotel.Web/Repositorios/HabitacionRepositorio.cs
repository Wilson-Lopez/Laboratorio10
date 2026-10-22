using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Habitaciones activas con su tipo; filtros opcionales por tipo, capacidad mínima y precio máximo; orden por número
    public async Task<List<Habitacion>> ListarActivasAsync(int? idTipo, int? capacidadMinima = null, decimal? precioMaximo = null)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoNavigation)
            .Where(h => h.Activo);

        if (idTipo.HasValue)
            consulta = consulta.Where(h => h.IdTipo == idTipo.Value);

        if (capacidadMinima.HasValue)
            consulta = consulta.Where(h => h.IdTipoNavigation.Capacidad >= capacidadMinima.Value);

        if (precioMaximo.HasValue)
            consulta = consulta.Where(h => h.IdTipoNavigation.PrecioNoche <= precioMaximo.Value);

        return await consulta.OrderBy(h => h.Numero).ToListAsync();
    }

    // Ids de habitaciones ocupadas en una fecha (reserva Pendiente o Confirmada que cubre esa noche)
    public async Task<List<int>> ListarIdsOcupadasAsync(DateOnly fecha)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Where(r => (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada) &&
                        r.FechaIngreso <= fecha && r.FechaSalida > fecha)
            .Select(r => r.IdHabitacion)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Habitacion?> ObtenerActivaAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);
    }

    // Habitación con sus reservas vigentes (Pendiente/Confirmada que aún no terminan)
    public async Task<Habitacion?> ObtenerConReservasVigentesAsync(int id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoNavigation)
            .Include(h => h.Reservas
                    .Where(r => (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada) &&
                                r.FechaSalida > hoy)
                    .OrderBy(r => r.FechaIngreso))
                .ThenInclude(r => r.IdHuespedNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);
    }

    // Incluye habitaciones inactivas porque Numero es UNIQUE en la tabla
    public async Task<bool> ExisteNumeroAsync(string numero, int? excluirId = null)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.Numero == numero && h.IdHabitacion != (excluirId ?? 0));
    }

    public async Task<bool> TieneReservasVigentesAsync(int id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.IdHabitacion == id &&
                           (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada) &&
                           r.FechaSalida > hoy);
    }

    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .OrderBy(t => t.PrecioNoche)
            .ToListAsync();
    }

    // Tipos con al menos una habitación activa (para la página de inicio)
    public async Task<List<TipoHabitacion>> ListarTiposConHabitacionesAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Include(t => t.Habitacions.Where(h => h.Activo))
            .Where(t => t.Habitacions.Any(h => h.Activo))
            .OrderBy(t => t.PrecioNoche)
            .ToListAsync();
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.Activo = true;
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Habitacion habitacion)
    {
        var actual = await _context.Habitacions
            .FirstOrDefaultAsync(h => h.IdHabitacion == habitacion.IdHabitacion && h.Activo);
        if (actual == null)
            return false;

        actual.Numero = habitacion.Numero;
        actual.Piso = habitacion.Piso;
        actual.IdTipo = habitacion.IdTipo;
        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminación lógica: Activo = 0
    public async Task<bool> EliminarAsync(int id)
    {
        var habitacion = await _context.Habitacions
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);
        if (habitacion == null)
            return false;

        habitacion.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
