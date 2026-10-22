using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Models;

// Completa la clase generada por scaffold sin modificar su archivo
[ModelMetadataType(typeof(ReservaMetadata))]
public partial class Reserva
{
    public int Noches => FechaSalida.DayNumber - FechaIngreso.DayNumber;
}

public class ReservaMetadata
{
    [Display(Name = "Huésped")]
    [Required(ErrorMessage = "Seleccione un huésped.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un huésped.")]
    public int IdHuesped { get; set; }

    [Display(Name = "Habitación")]
    [Required(ErrorMessage = "Seleccione una habitación.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una habitación.")]
    public int IdHabitacion { get; set; }

    [Display(Name = "Fecha de ingreso")]
    [Required(ErrorMessage = "Ingrese la fecha de ingreso.")]
    public DateOnly FechaIngreso { get; set; }

    [Display(Name = "Fecha de salida")]
    [Required(ErrorMessage = "Ingrese la fecha de salida.")]
    public DateOnly FechaSalida { get; set; }

    [Display(Name = "Estado")]
    public string Estado { get; set; } = null!;

    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; }

    [Display(Name = "Observaciones")]
    [StringLength(200, ErrorMessage = "Las observaciones no pueden tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }
}
