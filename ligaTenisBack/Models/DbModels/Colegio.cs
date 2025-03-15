using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Colegio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int NumeroJugadores { get; set; }

    public virtual ICollection<Jugador> Jugadors { get; set; } = new List<Jugador>();

    public virtual ICollection<Partido> PartidoLocals { get; set; } = new List<Partido>();

    public virtual ICollection<Partido> PartidoVisitantes { get; set; } = new List<Partido>();
}
