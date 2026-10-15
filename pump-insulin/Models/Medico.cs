namespace pumpinsulin.Models;

public class Medico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Especialidad { get; set; } = "Endocrinología";
    public string Telefono { get; set; } = string.Empty;

    // Relación: Un médico tiene varios pacientes
    public List<Paciente> Pacientes { get; set; } = new();
}