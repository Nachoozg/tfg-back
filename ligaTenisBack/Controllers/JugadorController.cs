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

        // GET api/Jugador/colegio/5
        [HttpGet("colegio/{colegioId}")]
        public async Task<IActionResult> GetJugadoresPorColegio(int colegioId)
        {
            try
            {
                var jugadores = await _context.Jugadors
                                    .Where(j => j.ColegioId == colegioId)
                                    .ToListAsync();
                if (jugadores == null || jugadores.Count == 0)
                {
                    return NotFound(new { message = "No hay jugadores en este colegio." });
                }
                return Ok(jugadores);
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
                return Ok(new { message = "Jugador actualizado con éxito!" });
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
                return Ok(new { message = "Jugador eliminado con éxito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha enviado ningún archivo");
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var relativePath = Path.Combine("imagenes", fileName).Replace("\\", "/");
                return Ok(new { path = relativePath });
            }
            catch (Exception ex)
            {
                return BadRequest("Error al subir el archivo: " + ex.Message);
            }
        }
    }
}