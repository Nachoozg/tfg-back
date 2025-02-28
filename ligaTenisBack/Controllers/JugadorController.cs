using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ligaTenisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadorController : ControllerBase
    {
        private readonly LigatenisContext _context;

        public JugadorController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/<JugadorController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listJugadores = await _context.Jugadors.ToListAsync();

                return Ok(listJugadores);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<JugadorController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var jugador = await _context.Jugadors.FindAsync(id);

                if (jugador == null)
                {
                    return NotFound();
                }

                return Ok(jugador);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<JugadorController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Jugador jugador)
        {
            try
            {
                _context.Add(jugador);
                await _context.SaveChangesAsync();

                return Ok(jugador);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<JugadorController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Jugador jugador)
        {
            try
            {
                if (id != jugador.Id)
                {
                    return BadRequest();
                }

                _context.Update(jugador);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Jugador actualizado con exito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<JugadorController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var jugador = await _context.Jugadors.FindAsync(id);

                if (jugador == null)
                {
                    return NotFound();
                }

                _context.Jugadors.Remove(jugador);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Jugador eliminado con exito! " });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
