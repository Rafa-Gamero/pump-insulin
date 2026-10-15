namespace pumpinsulin.Models;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Dni { get; set; } = string.Empty;

    // Credenciales de acceso
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public int? MedicoId { get; set; }
    public Medico? Medico { get; set; }

    public Dispositivo? Dispositivo { get; set; }
}