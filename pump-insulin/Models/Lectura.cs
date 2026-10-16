using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pumpinsulin.Models;

public class Lectura
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Fecha y Hora")]
    public DateTime FechaHora { get; set; }

    [Required]
    [Display(Name = "Glucosa (mg/dL)")]
    public double NivelGlucosaMgDl { get; set; }

    [Required]
    [Display(Name = "Insulina Suministrada (U)")]
    public double InsulinaSuministradaU { get; set; }

    [Display(Name = "Hidratos de Carbono (g)")]
    public double? HidratosCarbonoG { get; set; }

    [Display(Name = "Ratio Insulina/HC (U/10g o U/r)")]
    public double? RatioInsulina { get; set; }

    public EstadoAlerta EstadoAlerta { get; set; } = EstadoAlerta.Normal;

    // --- ALIAS PARA RESOLVER TODOS LOS ERRORES AL INSTANTE ---
    [NotMapped]
    public EstadoAlerta Alerta
    {
        get => EstadoAlerta;
        set => EstadoAlerta = value;
    }
    // ---------------------------------------------------------

    public int DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }
}