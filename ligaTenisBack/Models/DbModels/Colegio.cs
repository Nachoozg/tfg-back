using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Colegio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int NumeroJugadores { get; set; }
}
