using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class TipoHabitacion
{
    public int IdTipo { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal PrecioNoche { get; set; }

    public int Capacidad { get; set; }

    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();
}
