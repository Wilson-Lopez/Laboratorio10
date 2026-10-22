using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Models;

// Completa la clase generada por scaffold sin modificar su archivo
[ModelMetadataType(typeof(HabitacionMetadata))]
public partial class Habitacion
{
}

public class HabitacionMetadata
{
    [Display(Name = "Número")]
    [Required(ErrorMessage = "Ingrese el número de la habitación.")]
    [StringLength(10, ErrorMessage = "El número no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z-]+$", ErrorMessage = "Use solo letras, números o guion.")]
    public string Numero { get; set; } = null!;

    [Display(Name = "Piso")]
    [Required(ErrorMessage = "Ingrese el piso.")]
    [Range(1, 50, ErrorMessage = "El piso debe estar entre {1} y {2}.")]
    public int Piso { get; set; }

    [Display(Name = "Tipo de habitación")]
    [Required(ErrorMessage = "Seleccione el tipo de habitación.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el tipo de habitación.")]
    public int IdTipo { get; set; }
}
