using Hotel.Web.Models;

namespace Hotel.Web.Repositorios;

// Ciclo de vida de una reserva:
//   Pendiente  -> Confirmada | Cancelada
//   Confirmada -> Finalizada | Cancelada   (Finalizada solo si ya llegó la fecha de ingreso)
//   Cancelada y Finalizada son estados finales
public static class EstadoReserva
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";
    public const string Finalizada = "Finalizada";

    public static readonly string[] Todos = { Pendiente, Confirmada, Cancelada, Finalizada };

    public static bool PuedeConfirmar(Reserva r) => r.Estado == Pendiente;

    public static bool PuedeCancelar(Reserva r) => r.Estado is Pendiente or Confirmada;

    public static bool PuedeFinalizar(Reserva r, DateOnly hoy) => r.Estado == Confirmada && r.FechaIngreso <= hoy;

    public static bool PuedeCambiarA(Reserva r, string nuevoEstado, DateOnly hoy) => nuevoEstado switch
    {
        Confirmada => PuedeConfirmar(r),
        Cancelada => PuedeCancelar(r),
        Finalizada => PuedeFinalizar(r, hoy),
        _ => false
    };

    public static string CssBadge(string estado) => estado switch
    {
        Pendiente => "bg-warning text-dark",
        Confirmada => "bg-primary",
        Cancelada => "bg-danger",
        Finalizada => "bg-success",
        _ => "bg-secondary"
    };
}
