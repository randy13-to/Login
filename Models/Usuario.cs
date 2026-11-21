using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    [Column("ContrasenaHash")]
    public string Contrasena { get; set; } = string.Empty;

    public string Rol { get; set; } = "Usuario";
    public bool CorreoVerificado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}