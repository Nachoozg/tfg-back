using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Jugador
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Apellidos { get; set; }

    public string? Edad { get; set; }

    public int? ColegioId { get; set; }

    public string? ImagenJugador { get; set; }

    public virtual Colegio? Colegio { get; set; }

    public virtual ICollection<Partido> PartidoJugadorLocals { get; set; } = new List<Partido>();

    public virtual ICollection<Partido> PartidoJugadorVisitantes { get; set; } = new List<Partido>();
}
