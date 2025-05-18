// EstadisticasController.cs
using ligaTenisBack.Dtos;
using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EstadisticasJugadorController : ControllerBase
{
    private readonly LigatenisContext _context;
    public EstadisticasJugadorController(LigatenisContext context)
        => _context = context;

    // GET api/estadisticas/jugador/5
    [HttpGet("jugador/{jugadorId}")]
    public async Task<IActionResult> GetStatsJugador(int jugadorId)
    {
        // Primero traemos los partidos donde haya jugado ese jugador
        var stats = await _context.Partidos
          .Where(p => p.JugadorLocalId == jugadorId || p.JugadorVisitanteId == jugadorId)
          .Select(p => new {
              IsLocal = p.JugadorLocalId == jugadorId,
              LocalScore = p.ResultadoLocal ?? 0,
              VisitorScore = p.ResultadoVisitante ?? 0
          })
          .ToListAsync();

        var jugados = stats.Count;
        var victorias = stats.Count(x =>
            x.IsLocal ? x.LocalScore > x.VisitorScore
                       : x.VisitorScore > x.LocalScore
        );
        var derrotas = jugados - victorias;
        var porcentaje = jugados > 0
          ? Math.Round(victorias * 100.0 / jugados, 1)
          : 0.0;

        var dto = new JugadorEstadisticasDto
        {
            JugadorId = jugadorId,
            PartidosJugados = jugados,
            Victorias = victorias,
            Derrotas = derrotas,
            PorcentajeVictoria = porcentaje
        };
        return Ok(dto);
    }
}
