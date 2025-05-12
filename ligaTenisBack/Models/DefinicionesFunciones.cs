using System.Text.Json.Serialization;

namespace ligaTenisBack.Models
{
    // Descripción de cada "herramienta" que el modelo puede invocar
    public class DefinicionFuncion
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;

        [JsonPropertyName("parameters")]
        public object Parameters { get; set; } = default!;
    }

    // Argumentos que recibirá cada función
    public record ListarColegiosArgs();
    public record ListarPartidosArgs(bool soloFuturos);
    public record ListarJugadoresArgs(int? colegioId);
    public record ObtenerClasificacionArgs();
}
