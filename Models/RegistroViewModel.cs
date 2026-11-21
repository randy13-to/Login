using System.ComponentModel.DataAnnotations;

namespace LoginApi.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario o correo electrónico.")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;
}

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z0-9._]{3,30}$",
        ErrorMessage = "Usa de 3 a 30 caracteres (solo letras, números, puntos o guion bajo, sin espacios).")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de WhatsApp es obligatorio.")]
    [RegularExpression(@"^\+[1-9]\d{7,14}$",
        ErrorMessage = "Usa formato internacional con código de país (ejemplo: +50255551234).")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.#_-]).{8,}$",
        ErrorMessage = "Debe tener mínimo 8 caracteres, una mayúscula, una minúscula, un número y un símbolo (@$!%*?&.#_-).")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}