using System.Net;
using System.Security.Claims;
using LoginApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Controllers;

public class AuthController : Controller
{
    private readonly LoginContext _context;

    public AuthController(LoginContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identificador = model.Identificador.Trim().ToLower();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
            u.Correo.ToLower() == identificador || u.NombreUsuario.ToLower() == identificador);

        if (usuario is null || !usuario.Activo)
        {
            ModelState.AddModelError(string.Empty, "Usuario/correo incorrecto o cuenta inactiva.");
            return View(model);
        }

        bool passwordValida = !string.IsNullOrWhiteSpace(usuario.Contrasena) && usuario.Contrasena.StartsWith("$2")
            ? BCrypt.Net.BCrypt.Verify(model.Contrasena, usuario.Contrasena)
            : usuario.Contrasena == model.Contrasena;

        if (!passwordValida)
        {
            ModelState.AddModelError(string.Empty, "La contraseña ingresada es incorrecta.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new("NombreCompleto", usuario.Nombre),
            new(ClaimTypes.Role, usuario.Rol ?? "Usuario")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var correoLimpio = model.Correo.Trim().ToLower();
        var usuarioLimpio = model.NombreUsuario.Trim();

        // Verificar que el dominio del correo exista realmente en internet (DNS)
        if (!await DominioCorreoExisteAsync(correoLimpio))
        {
            ModelState.AddModelError(nameof(model.Correo),
                "El dominio de este correo no parece existir. Verifica que esté bien escrito (ej. @gmail.com).");
            return View(model);
        }

        if (await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoLimpio))
        {
            ModelState.AddModelError(nameof(model.Correo), "Este correo electrónico ya está registrado.");
            return View(model);
        }

        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == usuarioLimpio.ToLower()))
        {
            ModelState.AddModelError(nameof(model.NombreUsuario), "Ese nombre de usuario ya está en uso. Prueba con otro.");
            return View(model);
        }

        var nuevoUsuario = new Usuario
        {
            Nombre = model.Nombre.Trim(),
            NombreUsuario = usuarioLimpio,
            Correo = correoLimpio,
            Telefono = model.Telefono.Trim(),
            Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena),
            Rol = "Usuario",
            CorreoVerificado = true,
            Activo = true,
            FechaRegistro = DateTime.Now
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "¡Cuenta creada con éxito! Ya puedes iniciar sesión con tu usuario o correo.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static async Task<bool> DominioCorreoExisteAsync(string correo)
    {
        try
        {
            var partes = correo.Split('@');
            if (partes.Length != 2) return false;
            var direcciones = await Dns.GetHostAddressesAsync(partes[1]);
            return direcciones.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}