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

        // POST: api/Clasificacion/Actualizar
        [HttpPost("Actualizar")]
        public async Task<IActionResult> ActualizarClasificacion()
        {
            try
            {
                var partidos = await _context.Partidos
                    .Include(p => p.Local)
                    .Include(p => p.Visitante)
                    .ToListAsync();

                var clasificacionDict = new Dictionary<int, Clasificacion>();

                foreach (var partido in partidos)
                {
                    if (partido.LocalId.HasValue && partido.Local != null)
                    {
                        if (!clasificacionDict.ContainsKey(partido.LocalId.Value))
                        {
                            clasificacionDict[partido.LocalId.Value] = new Clasificacion
                            {
                                EquipoId = partido.LocalId.Value,
                                NombreEquipo = partido.Local.Nombre,
                                PartidosJugados = 0,
                                Victorias = 0,
                                Derrotas = 0,
                                Puntos = 0
                            };
                        }
                    }
                    if (partido.VisitanteId.HasValue && partido.Visitante != null)
                    {
                        if (!clasificacionDict.ContainsKey(partido.VisitanteId.Value))
                        {
                            clasificacionDict[partido.VisitanteId.Value] = new Clasificacion
                            {
                                EquipoId = partido.VisitanteId.Value,
                                NombreEquipo = partido.Visitante.Nombre,
                                PartidosJugados = 0,
                                Victorias = 0,
                                Derrotas = 0,
                                Puntos = 0
                            };
                        }
                    }

                    if (partido.ResultadoLocal.HasValue && partido.ResultadoVisitante.HasValue)
                    {
                        if (partido.LocalId.HasValue)
                        {
                            var local = clasificacionDict[partido.LocalId.Value];
                            local.PartidosJugados++;
                            if (partido.ResultadoLocal > partido.ResultadoVisitante)
                            {
                                local.Victorias++;
                                local.Puntos += 2;
                            }
                            else if (partido.ResultadoLocal < partido.ResultadoVisitante)
                            {
                                local.Derrotas++;
                                local.Puntos += 1;
                            }
                        }
                        if (partido.VisitanteId.HasValue)
                        {
                            var visitante = clasificacionDict[partido.VisitanteId.Value];
                            visitante.PartidosJugados++;
                            if (partido.ResultadoVisitante > partido.ResultadoLocal)
                            {
                                visitante.Victorias++;
                                visitante.Puntos += 2;
                            }
                            else if (partido.ResultadoVisitante < partido.ResultadoLocal)
                            {
                                visitante.Derrotas++;
                                visitante.Puntos += 1;
                            }
                        }
                    }
                }

                var listaClasificacion = clasificacionDict.Values
                    .OrderByDescending(c => c.Puntos)
                    .ToList();

                var registrosExistentes = await _context.Clasificacions.ToListAsync();
                _context.Clasificacions.RemoveRange(registrosExistentes);
                await _context.SaveChangesAsync();

                _context.Clasificacions.AddRange(listaClasificacion);
                await _context.SaveChangesAsync();

                return Ok(listaClasificacion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Clasificacion
        [HttpGet]
        public async Task<IActionResult> GetClasificacion()
        {
            try
            {
                var clasificaciones = await _context.Clasificacions
                    .OrderByDescending(c => c.Puntos)
                    .ToListAsync();

                return Ok(clasificaciones);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
