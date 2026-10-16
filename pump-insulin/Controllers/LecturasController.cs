using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

[Authorize]
public class LecturasController : Controller
{
    private readonly ApplicationDbContext _context;

    public LecturasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Lecturas
    public async Task<IActionResult> Index()
    {
        var query = _context.Lecturas
            .Include(l => l.Dispositivo)
                .ThenInclude(d => d!.Paciente)
            .AsQueryable();

        if (User.IsInRole("Paciente"))
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            query = query.Where(l => l.Dispositivo != null && l.Dispositivo.PacienteId == userId);
        }

        var lecturas = await query.OrderByDescending(l => l.FechaHora).ToListAsync();
        return View(lecturas);
    }

    // GET: Lecturas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var lectura = await _context.Lecturas
            .Include(l => l.Dispositivo)
                .ThenInclude(d => d!.Paciente)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (lectura == null) return NotFound();

        if (User.IsInRole("Paciente"))
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (lectura.Dispositivo?.PacienteId != userId)
            {
                return Forbid();
            }
        }

        return View(lectura);
    }

    // GET: Lecturas/Create (Registro individual)
    [Authorize(Roles = "Paciente")]
    public async Task<IActionResult> Create()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dispositivo = await _context.Dispositivos.FirstOrDefaultAsync(d => d.PacienteId == userId);

        if (dispositivo == null)
        {
            TempData["Error"] = "No tienes una bomba de insulina asignada. Contacta con tu médico para que te asigne un dispositivo antes de registrar lecturas.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DispositivoInfo = $"{dispositivo.Modelo} (S/N: {dispositivo.NumeroSerie})";

        return View(new Lectura
        {
            FechaHora = DateTime.Now,
            DispositivoId = dispositivo.Id
        });
    }

    // POST: Lecturas/Create
    [HttpPost]
    [Authorize(Roles = "Paciente")]
    public async Task<IActionResult> Create(Lectura lectura)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dispositivo = await _context.Dispositivos.FirstOrDefaultAsync(d => d.PacienteId == userId);

        if (dispositivo == null)
        {
            TempData["Error"] = "No se encontró ningún dispositivo asignado a tu cuenta.";
            return RedirectToAction(nameof(Index));
        }

        lectura.DispositivoId = dispositivo.Id;

        // Clasificación automática de alerta médica
        if (lectura.NivelGlucosaMgDl <= 65)
        {
            lectura.EstadoAlerta = EstadoAlerta.Hipoglucemia;
            TempData["Aviso"] = $"⚠️ ¡ATENCIÓN! Se ha detectado HIPOGLUCEMIA ({lectura.NivelGlucosaMgDl} mg/dL). Toma hidratos de absorción rápida y consulta con tu médico.";
            TempData["TipoAviso"] = "danger";
        }
        else if (lectura.NivelGlucosaMgDl >= 200)
        {
            lectura.EstadoAlerta = EstadoAlerta.Hiperglucemia;
            TempData["Aviso"] = $"⚠️ ¡ATENCIÓN! Se ha detectado HIPERGLUCEMIA ({lectura.NivelGlucosaMgDl} mg/dL). Revisa la dosis de insulina/bomba y el aporte hídrico.";
            TempData["TipoAviso"] = "warning";
        }
        else
        {
            lectura.EstadoAlerta = EstadoAlerta.Normal;
            TempData["Aviso"] = $"Nivel de glucosa normal ({lectura.NivelGlucosaMgDl} mg/dL). Lectura guardada correctamente.";
            TempData["TipoAviso"] = "success";
        }

        if (ModelState.IsValid)
        {
            _context.Lecturas.Add(lectura);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DispositivoInfo = $"{dispositivo.Modelo} (S/N: {dispositivo.NumeroSerie})";
        return View(lectura);
    }

    // --- ACCIÓN GET AÑADIDA: Permite abrir la página para subir el archivo ---
    [HttpGet]
    [Authorize(Roles = "Paciente")]
    public IActionResult SubirArchivo()
    {
        return View();
    }

    // POST: Lecturas/SubirArchivo (Procesamiento del CSV)
    [HttpPost]
    [Authorize(Roles = "Paciente")]
    public async Task<IActionResult> SubirArchivo(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            ViewBag.Error = "Por favor selecciona un archivo CSV válido.";
            return View();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dispositivo = await _context.Dispositivos.FirstOrDefaultAsync(d => d.PacienteId == userId);

        if (dispositivo == null)
        {
            ViewBag.Error = "No tienes ningún dispositivo asignado para vincular estas lecturas.";
            return View();
        }

        var nuevasLecturas = new List<Lectura>();

        using (var reader = new StreamReader(archivo.OpenReadStream()))
        {
            string? linea;
            bool esPrimeraLinea = true;

            while ((linea = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                if (esPrimeraLinea)
                {
                    esPrimeraLinea = false;
                    if (linea.Contains("Fecha", StringComparison.OrdinalIgnoreCase) ||
                        linea.Contains("Glucosa", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var partes = linea.Split(',');
                if (partes.Length >= 3)
                {
                    if (DateTime.TryParse(partes[0].Trim(), out var fecha) &&
                        double.TryParse(partes[1].Trim(), CultureInfo.InvariantCulture, out var glucosa) &&
                        double.TryParse(partes[2].Trim(), CultureInfo.InvariantCulture, out var insulina))
                    {
                        double? hidratos = null;
                        if (partes.Length >= 4 && double.TryParse(partes[3].Trim(), CultureInfo.InvariantCulture, out var h))
                        {
                            hidratos = h;
                        }

                        double? ratio = null;
                        if (partes.Length >= 5 && double.TryParse(partes[4].Trim(), CultureInfo.InvariantCulture, out var r))
                        {
                            ratio = r;
                        }

                        var alerta = EstadoAlerta.Normal;
                        if (glucosa <= 65) alerta = EstadoAlerta.Hipoglucemia;
                        else if (glucosa >= 200) alerta = EstadoAlerta.Hiperglucemia;

                        nuevasLecturas.Add(new Lectura
                        {
                            FechaHora = fecha,
                            NivelGlucosaMgDl = glucosa,
                            InsulinaSuministradaU = insulina,
                            HidratosCarbonoG = hidratos,
                            RatioInsulina = ratio,
                            EstadoAlerta = alerta,
                            DispositivoId = dispositivo.Id
                        });
                    }
                }
            }
        }

        if (nuevasLecturas.Any())
        {
            _context.Lecturas.AddRange(nuevasLecturas);
            await _context.SaveChangesAsync();
            TempData["Aviso"] = $"Se han importado {nuevasLecturas.Count} lecturas correctamente.";
            TempData["TipoAviso"] = "success";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Error = "No se pudieron procesar las líneas del archivo. Comprueba el formato esperado: FechaHora,Glucosa,Insulina,Hidratos,Ratio";
        return View();
    }

    // GET: Lecturas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var lectura = await _context.Lecturas
            .Include(l => l.Dispositivo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (lectura == null) return NotFound();

        if (User.IsInRole("Paciente"))
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (lectura.Dispositivo?.PacienteId != userId)
            {
                return Forbid();
            }
        }

        return View(lectura);
    }

    // POST: Lecturas/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lectura = await _context.Lecturas
            .Include(l => l.Dispositivo)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lectura != null)
        {
            if (User.IsInRole("Paciente"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (lectura.Dispositivo?.PacienteId != userId)
                {
                    return Forbid();
                }
            }

            _context.Lecturas.Remove(lectura);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Lecturas/Grafico
    [Authorize]
    public async Task<IActionResult> Grafico(int? pacienteId)
    {
        var query = _context.Lecturas
            .Include(l => l.Dispositivo)
                .ThenInclude(d => d!.Paciente)
            .AsQueryable();

        if (User.IsInRole("Paciente"))
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            query = query.Where(l => l.Dispositivo != null && l.Dispositivo.PacienteId == userId);
        }
        else if (User.IsInRole("Medico") && pacienteId.HasValue)
        {
            query = query.Where(l => l.Dispositivo != null && l.Dispositivo.PacienteId == pacienteId.Value);
            ViewBag.PacienteSeleccionado = await _context.Pacientes.FindAsync(pacienteId.Value);
        }

        var lecturas = await query
            .OrderBy(l => l.FechaHora)
            .ToListAsync();

        return View(lecturas);
    }
}