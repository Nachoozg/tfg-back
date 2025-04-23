namespace ligaTenisBack.Dtos
{
    public class PartidoDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Lugar { get; set; }
        public string Detalles { get; set; }
        public int? LocalId { get; set; }
        public int? VisitanteId { get; set; }
        public int? ResultadoLocal { get; set; }
        public int? ResultadoVisitante { get; set; }
        public ColegioDto Local { get; set; }
        public ColegioDto Visitante { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }
}
