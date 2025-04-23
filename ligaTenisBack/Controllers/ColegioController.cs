using ligaTenisBack.Dtos;
using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ligaTenisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColegioController : ControllerBase
    {
        private readonly LigatenisContext _context;

        public ColegioController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/<ColegioController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listColegios = await _context.Colegios
                    .Include(c => c.Jugadors)
                    .Select(c => new ColegioDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        ImagenColegio = c.ImagenColegio,
                        NumeroJugadores = c.Jugadors.Count
                    })
                    .ToListAsync();

                return Ok(listColegios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<ColegioController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var colegio = await _context.Colegios
                    .Include(c => c.Jugadors)
                    .Where(c => c.Id == id)
                    .Select(c => new ColegioDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        ImagenColegio = c.ImagenColegio,
                        NumeroJugadores = c.Jugadors.Count
                    })
                    .FirstOrDefaultAsync();

                if (colegio == null) return NotFound();
                return Ok(colegio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<ColegioController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ColegioDto colegioDto)
        {
            try
            {
                var colegio = new Colegio
                {
                    Nombre = colegioDto.Nombre,
                    NumeroJugadores = colegioDto.NumeroJugadores,
                    ImagenColegio = colegioDto.ImagenColegio
                };

                _context.Add(colegio);
                await _context.SaveChangesAsync();

                return Ok(colegio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit] // Evita limitaciones en el tamaño del archivo
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

                // Copia el archivo hacia la carpeta especificada
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("imagenes", fileName).Replace("\\", "/");
                return Ok(new { path = relativePath });
            }
            catch (Exception ex)
            {
                // Regresa el error para poder identificarlo
                return BadRequest("Error al subir el archivo: " + ex.Message);
            }
        }

        // PUT api/<ColegioController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ColegioDto colegioDto)
        {
            try
            {
                if (id != colegioDto.Id)
                {
                    return BadRequest();
                }

                var colegio = await _context.Colegios.FindAsync(id);
                if (colegio == null)
                {
                    return NotFound();
                }

                colegio.Nombre = colegioDto.Nombre;
                colegio.NumeroJugadores = colegioDto.NumeroJugadores;
                colegio.ImagenColegio = colegioDto.ImagenColegio;

                _context.Update(colegio);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Colegio actualizado con éxito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<ColegioController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var colegio = await _context.Colegios.FindAsync(id);

                if (colegio == null)
                {
                    return NotFound();
                }

                _context.Colegios.Remove(colegio);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Colegio eliminado con éxito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}