using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Models;

// Completa la clase generada por scaffold sin modificar su archivo
[ModelMetadataType(typeof(HuespedMetadata))]
public partial class Huesped
{
    public string NombreCompleto => $"{Apellidos}, {Nombres}";
}

public class HuespedMetadata
{
    [Display(Name = "Nombres")]
    [Required(ErrorMessage = "Ingrese los nombres.")]
    [StringLength(80, ErrorMessage = "Los nombres no pueden tener más de {1} caracteres.")]
    public string Nombres { get; set; } = null!;

    [Display(Name = "Apellidos")]
    [Required(ErrorMessage = "Ingrese los apellidos.")]
    [StringLength(80, ErrorMessage = "Los apellidos no pueden tener más de {1} caracteres.")]
    public string Apellidos { get; set; } = null!;

    [Display(Name = "Documento (DNI / CE / Pasaporte)")]
    [Required(ErrorMessage = "Ingrese el número de documento.")]
    [StringLength(15, MinimumLength = 8, ErrorMessage = "El documento debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z]+$", ErrorMessage = "El documento solo puede contener letras y números.")]
    public string Documento { get; set; } = null!;

    [Display(Name = "Correo electrónico")]
    [StringLength(100, ErrorMessage = "El correo no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    public string? Email { get; set; }

    [Display(Name = "Teléfono")]
    [StringLength(20, ErrorMessage = "El teléfono no puede tener más de {1} caracteres.")]
    [RegularExpression(@"^\+?[0-9 ]{6,20}$", ErrorMessage = "Ingrese un teléfono válido (solo números, opcional +).")]
    public string? Telefono { get; set; }
}
