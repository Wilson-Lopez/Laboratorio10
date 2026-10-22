using System.Data;
using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Reservas con huésped y habitación; filtros opcionales por estado y texto (huésped, documento o número)
    public async Task<List<Reserva>> ListarAsync(string? estado, string? buscar)
    {
        var consulta = _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(r => r.Estado == estado);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(r => r.IdHuespedNavigation.Nombres.Contains(buscar) ||
                                           r.IdHuespedNavigation.Apellidos.Contains(buscar) ||
                                           r.IdHuespedNavigation.Documento.Contains(buscar) ||
                                           r.IdHabitacionNavigation.Numero == buscar);
        }

        return await consulta
            .OrderByDescending(r => r.FechaIngreso)
            .ThenByDescending(r => r.IdReserva)
            .ToListAsync();
    }

    public async Task<Reserva?> ObtenerAsync(int id)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoNavigation)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
    }

    // Habitaciones activas sin reservas que se crucen con el rango pedido (una sola consulta)
    public async Task<List<Habitacion>> ListarDisponiblesAsync(DateOnly ingreso, DateOnly salida,
        int? capacidadMinima = null, int? idTipo = null)
    {
        return await FiltrarHabitaciones(capacidadMinima, idTipo)
            .Where(h => !h.Reservas.Any(r => r.Estado != EstadoReserva.Cancelada &&
                                             r.FechaIngreso < salida &&
                                             r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    // Habitaciones activas que sí tienen una reserva cruzada en el rango (para mostrarlas como "No disponible")
    public async Task<List<Habitacion>> ListarNoDisponiblesAsync(DateOnly ingreso, DateOnly salida,
        int? capacidadMinima = null, int? idTipo = null)
    {
        return await FiltrarHabitaciones(capacidadMinima, idTipo)
            .Where(h => h.Reservas.Any(r => r.Estado != EstadoReserva.Cancelada &&
                                            r.FechaIngreso < salida &&
                                            r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    private IQueryable<Habitacion> FiltrarHabitaciones(int? capacidadMinima, int? idTipo)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoNavigation)
            .Where(h => h.Activo);

        if (capacidadMinima.HasValue)
            consulta = consulta.Where(h => h.IdTipoNavigation.Capacidad >= capacidadMinima.Value);

        if (idTipo.HasValue)
            consulta = consulta.Where(h => h.IdTipo == idTipo.Value);

        return consulta;
    }

    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .OrderBy(t => t.PrecioNoche)
            .ToListAsync();
    }

    public async Task<bool> HabitacionReservadaAsync(int idHabitacion, DateOnly ingreso, DateOnly salida)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.IdHabitacion == idHabitacion &&
                           r.Estado != EstadoReserva.Cancelada &&
                           r.FechaIngreso < salida &&
                           r.FechaSalida > ingreso);
    }

    public async Task<bool> HuespedActivoAsync(int idHuesped)
    {
        return await _context.Huespeds.AsNoTracking().AnyAsync(h => h.IdHuesped == idHuesped && h.Activo);
    }

    public async Task<bool> HabitacionActivaAsync(int idHabitacion)
    {
        return await _context.Habitacions.AsNoTracking().AnyAsync(h => h.IdHabitacion == idHabitacion && h.Activo);
    }

    public async Task<List<Huesped>> ListarHuespedesActivosAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ListarHabitacionesActivasAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    // Vuelve a verificar el cruce dentro de una transacción serializable para que
    // dos usuarios no reserven la misma habitación en el mismo rango a la vez.
    public async Task<bool> CrearAsync(Reserva reserva)
    {
        await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (await HabitacionReservadaAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
            return false;

        reserva.Estado = EstadoReserva.Pendiente;
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return true;
    }

    // Aplica la transición solo si está permitida (ver EstadoReserva)
    public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (reserva == null || !EstadoReserva.PuedeCambiarA(reserva, nuevoEstado, hoy))
            return false;

        reserva.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return true;
    }
}
