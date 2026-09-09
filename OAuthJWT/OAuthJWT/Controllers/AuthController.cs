using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OAuthJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            string rol;

            // Usuarios de prueba
            if (request.Usuario == "admin" && request.Password == "1234")
            {
                rol = "Administrador";
            }
            else if (request.Usuario == "usuario" && request.Password == "1234")
            {
                rol = "Usuario";
            }
            else
            {
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
            }

            // Datos (Claims) que viajarán encriptados dentro del token
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Usuario),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Clave secreta para firmar el token
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credenciales = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            // Generar el token JWT
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])
                ),
                signingCredentials: credenciales
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                usuario = request.Usuario,
                rol = rol,
                expiraEnMinutos = _configuration["Jwt:ExpireMinutes"]
            });
        }
    }

    public class LoginRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}