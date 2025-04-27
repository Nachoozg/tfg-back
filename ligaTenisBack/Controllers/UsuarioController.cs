using ligaTenisBack.Models.DbModels;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ligaTenisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly LigatenisContext _context;

        public UsuarioController(LigatenisContext context)
        {
            _context = context;
        }

        // GET: api/Usuario/pendientes
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            var pendientes = await _context.Usuarios
                .Where(u => u.Aprobado != 1)
                .Select(u => new {
                    u.Id,
                    u.Nombre,
                    u.Apellidos,
                    u.Mail,
                    Rol = u.Rol!.Nombre
                })
                .ToListAsync();

            return Ok(pendientes);
        }

        // POST: api/Usuario/aprobar/5
        [HttpPost("aprobar/{id}")]
        public async Task<IActionResult> Aprobar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Aprobado = 1;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuario aprobado con éxito" });
        }

        // GET: api/<UsuarioController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listUsuarios = await _context.Usuarios.ToListAsync();

                return Ok(listUsuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<UsuarioController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound();
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // POST api/<UsuarioController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            try
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<UsuarioController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
        {
            try
            {
                if (id != usuario.Id)
                {
                    return BadRequest();
                }

                _context.Update(usuario);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario actualizado con exito!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Registro
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre)
                || string.IsNullOrWhiteSpace(usuario.Password)
                || string.IsNullOrWhiteSpace(usuario.Apellidos)
                || string.IsNullOrWhiteSpace(usuario.Mail))
            {
                return BadRequest("Todos los campos son obligatorios");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Mail == usuario.Mail))
                return BadRequest("El correo ya está registrado.");

            if (!EsPasswordValida(usuario.Password))
                return BadRequest("La contraseña debe tener 8 caracteres o más e incluir letras, números y un símbolo");

            usuario.Password = HashPassword(usuario.Password);
            usuario.Aprobado = 0;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tu solicitud de registro está pendiente de aprobación" });
        }

        // Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var u = await _context.Usuarios
                .Include(x => x.Rol)
                .FirstOrDefaultAsync(x => x.Mail == request.Mail);

            if (u == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            if (u.Aprobado != 1)
                return Unauthorized("Tu cuenta aún no ha sido aprobada");

            var hashNueva = HashPassword(request.Password);
            if (hashNueva != u.Password)
                return Unauthorized("Usuario o contraseña incorrectos");

            return Ok(new
            {
                user = new
                {
                    u.Id,
                    u.Nombre,
                    u.Mail,
                    RolId = u.RolId,
                    RolNombre = u.Rol!.Nombre
                }
            });
        }

        public class LoginRequest
        {
            public string Mail { get; set; }
            public string Password { get; set; }
        }

        private bool EsPasswordValida(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsLetter)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }

        private string HashPassword(string password)
        {
            SHA256CryptoServiceProvider provider = new SHA256CryptoServiceProvider();

            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashedBytes = provider.ComputeHash(inputBytes);

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < hashedBytes.Length; i++)
                output.Append(hashedBytes[i].ToString("x2").ToLower());

            return output.ToString();
        }
    }
}
