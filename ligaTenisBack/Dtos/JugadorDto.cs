namespace ligaTenisBack.Dtos
{
    public class JugadorDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Edad { get; set; }
        public int? ColegioId { get; set; }
        public IFormFile? Imagen { get; set; }
    }
}
