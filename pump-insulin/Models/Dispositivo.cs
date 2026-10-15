namespace pumpinsulin.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public DateTime FechaActivacion { get; set; } = DateTime.UtcNow;

    public int? PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public List<Lectura> Lecturas { get; set; } = new();
}