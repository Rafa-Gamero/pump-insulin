using System.ComponentModel.DataAnnotations;

namespace pumpinsulin.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = "Paciente"; // "Medico" o "Paciente"
}

public class RegistroViewModel
{
    [Required]
    public string Rol { get; set; } = "Paciente"; // "Medico" o "Paciente"

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Campos condicionales
    public string? Especialidad { get; set; } // Médico
    public string? Telefono { get; set; }     // Médico
    public string? Dni { get; set; }          // Paciente
    public DateTime? FechaNacimiento { get; set; } // Paciente
    public int? MedicoId { get; set; }        // Paciente elige a su médico
}