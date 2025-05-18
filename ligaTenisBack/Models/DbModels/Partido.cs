using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Partido
{
    public int Id { get; set; }

    public DateOnly Fecha { get; set; }

    public string Lugar { get; set; } = null!;

    public string Detalles { get; set; } = null!;

    public int? LocalId { get; set; }

    public int? VisitanteId { get; set; }

    public int? ResultadoLocal { get; set; }

    public int? ResultadoVisitante { get; set; }

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public int? JugadorLocalId { get; set; }

    public int? JugadorVisitanteId { get; set; }

    public virtual Jugador? JugadorLocal { get; set; }

    public virtual Jugador? JugadorVisitante { get; set; }

    public virtual Colegio? Local { get; set; }

    public virtual Colegio? Visitante { get; set; }
}
