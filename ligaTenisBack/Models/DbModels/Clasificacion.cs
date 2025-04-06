using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Clasificacion
{
    public int? EquipoId { get; set; }

    public string? NombreEquipo { get; set; }

    public int? PartidosJugados { get; set; }

    public int? Victorias { get; set; }

    public int? Derrotas { get; set; }

    public int? Puntos { get; set; }
}
