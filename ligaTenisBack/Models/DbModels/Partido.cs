using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Partido
{
    public int Id { get; set; }

    public DateOnly Fecha { get; set; }

    public string Lugar { get; set; } = null!;

    public string Detalles { get; set; } = null!;
}
