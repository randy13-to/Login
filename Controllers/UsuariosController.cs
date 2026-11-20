using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using LoginApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly LoginContext _context;
    private readonly IConfiguration _configuration;

    public UsuariosController(LoginContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // GET: api/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
    {
        return await _context.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Correo,
                u.Rol,
                u.Activo
            })
            .ToListAsync();
    }

    // GET: api/usuarios/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound(new { mensaje = $"No se encontró el usuario con Id {id}." });
        }

        return Ok(new
        {
            usuario.Id,
            usuario.Nombre,
            usuario.Correo,
            usuario.Rol,
            usuario.Activo
        });
    }

    // POST: api/usuarios
    [HttpPost]
    public async Task<ActionResult<object>> PostUsuario([FromBody] Usuario usuario)
    {
        if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
        {
            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
        }

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, new
        {
            usuario.Id,
            usuario.Nombre,
            usuario.Correo,
            usuario.Rol,
            usuario.Activo
        });
    }

    // PUT: api/usuarios/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest(new { mensaje = "El Id de la URL no coincide con el del cuerpo." });
        }

        var existente = await _context.Usuarios.FindAsync(id);
        if (existente is null)
        {
            return NotFound(new { mensaje = $"No se encontró el usuario con Id {id}." });
        }

        existente.Nombre = usuario.Nombre;
        existente.Correo = usuario.Correo;
        existente.Rol = usuario.Rol;
        existente.Activo = usuario.Activo;

        if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
        {
            existente.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente.",
            usuario = new
            {
                existente.Id,
                existente.Nombre,
                existente.Correo,
                existente.Rol,
                existente.Activo
            }
        });
    }

    // DELETE: api/usuarios/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return NotFound(new { mensaje = $"No se encontró el usuario con Id {id}." });
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/usuarios/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest(new { mensaje = "Debes ingresar correo y contraseña." });
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (usuario is null || !usuario.Activo)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas o usuario inactivo." });
        }

        bool passwordValida = !string.IsNullOrWhiteSpace(usuario.Contrasena) && usuario.Contrasena.StartsWith("$2")
            ? BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.Contrasena)
            : usuario.Contrasena == request.Contrasena;

        if (!passwordValida)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }

        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["ExpireMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol ?? "Usuario")
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return Ok(new
        {
            mensaje = "Inicio de sesión exitoso.",
            token = tokenString,
            expiracion = expira,
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Correo,
                usuario.Rol,
                usuario.Activo
            }
        });
    }
}