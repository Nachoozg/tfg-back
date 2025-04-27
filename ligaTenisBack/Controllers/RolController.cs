using ligaTenisBack.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ligaTenisBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly LigatenisContext _context;
        public RolController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/Rol
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roles = await _context.Rols
                                      .Select(r => new { r.Id, r.Nombre })
                                      .ToListAsync();
            return Ok(roles);
        }
    }
}
