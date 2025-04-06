using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ligaTenisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidoController : ControllerBase
    {
        private readonly LigatenisContext _context;

        public PartidoController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/<PartidoController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listPartidos = await _context.Partidos.ToListAsync();

                return Ok(listPartidos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<PartidoController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var partido = await _context.Partidos.FindAsync(id);

                if (partido == null)
                {
                    return NotFound();
                }

                return Ok(partido);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        //// POST api/<PartidoController>
        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] Partido partido)
        //{
        //    try
        //    {
        //        _context.Add(partido);
        //        await _context.SaveChangesAsync();

        //        return Ok(partido);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        // POST api/<PartidoController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Partido partido)
        {
            try
            {
                if (partido.LocalId == partido.VisitanteId)
                {
                    return BadRequest("El jugador local no puede ser el mismo que el visitante.");
                }

                var partidosEnFecha = await _context.Partidos
                    .Where(p => p.Fecha == partido.Fecha && (p.LocalId == partido.LocalId || p.VisitanteId == partido.VisitanteId))
                    .ToListAsync();

                if (partidosEnFecha.Any())
                {
                    return BadRequest("El jugador no puede jugar más de un partido en la misma fecha.");
                }

                _context.Add(partido);
                await _context.SaveChangesAsync();

                return Ok(partido);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // PUT api/<PartidoController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Partido partido)
        {
            try
            {
                if (id != partido.Id)
                {
                    return BadRequest();
                }

                _context.Update(partido);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Partido actualizado con exito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // DELETE api/<PartidoController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var partido = await _context.Partidos.FindAsync(id);

                if (partido == null)
                {
                    return NotFound();
                }

                _context.Partidos.Remove(partido);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Partido eliminado con exito! " });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}