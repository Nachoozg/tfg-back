namespace ligaTenisBack.Dtos
{
    public class JugadorEstadisticasDto
    {
        public int JugadorId { get; set; }
        public int PartidosJugados { get; set; }
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public double PorcentajeVictoria { get; set; }
    }
}
