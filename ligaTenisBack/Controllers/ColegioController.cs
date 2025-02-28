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
                var listColegios = await _context.Colegios.ToListAsync();

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
                var colegio = await _context.Colegios.FindAsync(id);

                if (colegio == null)
                {
                    return NotFound();
                }

                return Ok(colegio);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // POST api/<ColegioController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Colegio colegio)
        {
            try
            {
                _context.Add(colegio);
                await _context.SaveChangesAsync();

                return Ok(colegio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<ColegioController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Colegio colegio)
        {
            try
            {
                if (id != colegio.Id)
                {
                    return BadRequest();
                }

                _context.Update(colegio);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Colegio actualizado con exito!" });
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
                return Ok(new { message = "Colegio eliminado con exito! " });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
