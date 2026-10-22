namespace Hotel.Web.Helpers;

// Contenido visual (foto, descripción, cama y extras) de cada tipo de habitación.
// Si se crea un tipo nuevo en la base de datos se usan los valores por defecto.
public record PresentacionTipo(string Imagen, string Descripcion, string Cama, string[] Extras)
{
    public static PresentacionTipo Para(string nombreTipo) => nombreTipo.Trim().ToLowerInvariant() switch
    {
        "simple" => new("simple.jpg", "Cómoda y funcional, ideal para viajes de trabajo o escapadas cortas.", "1 cama 1½ plazas", ["Escritorio", "Smart TV"]),
        "doble" => new("doble.jpg", "Espacio cálido con dos camas, perfecta para amigos o familia.", "2 camas", ["Minibar", "Smart TV"]),
        "matrimonial" => new("matrimonial.jpg", "Cama king, sala de estar y una atmósfera pensada para el descanso.", "1 cama king", ["Sala de estar", "Minibar"]),
        "suite" => new("suite.jpg", "Amplia, luminosa y con vistas: la experiencia más exclusiva del hotel.", "1 cama king", ["Sala privada", "Jacuzzi"]),
        _ => new("simple.jpg", "Una habitación pensada para que disfrutes tu estadía.", "Cama confortable", ["Smart TV"])
    };
}
