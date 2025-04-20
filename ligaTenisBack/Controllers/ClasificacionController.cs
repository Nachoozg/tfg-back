using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ligaTenisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClasificacionController : ControllerBase
    {
        private readonly LigatenisContext _context;

        public ClasificacionController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/Clasificacion
        [HttpGet]
        public async Task<IActionResult> GetClasificacion()
        {
            var partidos = await _context.Partidos
                .Include(p => p.Local)
                .Include(p => p.Visitante)
                .ToListAsync();

            var dict = new Dictionary<int, Clasificacion>();
            var colegios = await _context.Colegios.ToListAsync();

            foreach (var c in colegios)
            {
                dict[c.Id] = new Clasificacion
                {
                    EquipoId = c.Id,
                    NombreEquipo = c.Nombre,
                    PartidosJugados = 0,
                    Victorias = 0,
                    Derrotas = 0,
                    Puntos = 0
                };
            }

            foreach (var p in partidos)
            {
                if (!p.ResultadoLocal.HasValue || !p.ResultadoVisitante.HasValue)
                    continue;

                var local = dict[p.LocalId!.Value];
                var visitante = dict[p.VisitanteId!.Value];

                local.PartidosJugados++;
                visitante.PartidosJugados++;

                if (p.ResultadoLocal > p.ResultadoVisitante)
                {
                    local.Victorias++;
                    local.Puntos += 2;

                    visitante.Derrotas++;
                    visitante.Puntos += 1;
                }
                else if (p.ResultadoVisitante > p.ResultadoLocal)
                {
                    visitante.Victorias++;
                    visitante.Puntos += 2;

                    local.Derrotas++;
                    local.Puntos += 1;
                }
            }

            var lista = dict.Values
                           .OrderByDescending(c => c.Puntos)
                           .ThenByDescending(c => c.Victorias)
                           .ToList();

            return Ok(lista);
        }
    }
}