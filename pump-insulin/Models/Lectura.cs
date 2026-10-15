namespace pumpinsulin.Models;

public class Lectura
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public double NivelGlucosaMgDl { get; set; }       // Nivel glucosa en mg/dL
    public double InsulinaSuministradaU { get; set; }   // Unidades de insulina

    // Campo tipado con la enumeración
    public EstadoAlerta Alerta { get; set; } = EstadoAlerta.Normal;

    // Relación con Dispositivo
    public int DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }
}