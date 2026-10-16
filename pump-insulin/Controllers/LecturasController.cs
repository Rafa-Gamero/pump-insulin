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
    // El paciente solo ve sus lecturas; el médico ve todas con la información del paciente
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

        // Si es un paciente, asegurarse de que la lectura pertenezca a su dispositivo
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

    // GET: Lecturas/Create (Registro individual para el paciente)
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

    // Vincula automáticamente el dispositivo
    lectura.DispositivoId = dispositivo.Id;

    // Clasificación automática según los umbrales clínicos
    if (lectura.NivelGlucosaMgDl <= 65)
    {
        lectura.Alerta = EstadoAlerta.Hipoglucemia;
        TempData["Aviso"] = $"⚠️ ¡ATENCIÓN! Se ha detectado HIPOGLUCEMIA ({lectura.NivelGlucosaMgDl} mg/dL). Toma hidratos de absorción rápida y consulta con tu médico.";
        TempData["TipoAviso"] = "danger"; // Rojo
    }
    else if (lectura.NivelGlucosaMgDl >= 200)
    {
        lectura.Alerta = EstadoAlerta.Hiperglucemia;
        TempData["Aviso"] = $"⚠️ ¡ATENCIÓN! Se ha detectado HIPERGLUCEMIA ({lectura.NivelGlucosaMgDl} mg/dL). Revisa la dosis de insulina/bomba y el aporte hídrico.";
        TempData["TipoAviso"] = "warning"; // Amarillo / Naranja
    }
    else
    {
        lectura.Alerta = EstadoAlerta.Normal;
        TempData["Aviso"] = $"Nivel de glucosa normal ({lectura.NivelGlucosaMgDl} mg/dL). Lectura guardada correctamente.";
        TempData["TipoAviso"] = "success"; // Verde
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

    // GET: Lecturas/SubirArchivo (Carga masiva por archivo para el paciente)
    [Authorize(Roles = "Paciente")]
    public IActionResult SubirArchivo()
    {
        return View();
    }

    // POST: Lecturas/SubirArchivo
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

                // Saltar cabecera si incluye nombres de columnas
                if (esPrimeraLinea)
                {
                    esPrimeraLinea = false;
                    if (linea.Contains("Fecha", StringComparison.OrdinalIgnoreCase) ||
                        linea.Contains("Glucosa", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // Formato esperado: FechaHora,Glucosa,Insulina,Alerta(opcional)
                var partes = linea.Split(',');
                if (partes.Length >= 3)
                {
                    if (DateTime.TryParse(partes[0].Trim(), out var fecha) &&
                        double.TryParse(partes[1].Trim(), CultureInfo.InvariantCulture, out var glucosa) &&
                        double.TryParse(partes[2].Trim(), CultureInfo.InvariantCulture, out var insulina))
                    {
                        var alerta = EstadoAlerta.Normal;
                        if (partes.Length >= 4 && Enum.TryParse<EstadoAlerta>(partes[3].Trim(), true, out var parsedAlerta))
                        {
                            alerta = parsedAlerta;
                        }
                        else
                        {
                            if (glucosa < 70) alerta = EstadoAlerta.Hipoglucemia;
                            else if (glucosa > 180) alerta = EstadoAlerta.Hiperglucemia;
                        }

                        nuevasLecturas.Add(new Lectura
                        {
                            FechaHora = fecha,
                            NivelGlucosaMgDl = glucosa,
                            InsulinaSuministradaU = insulina,
                            Alerta = alerta,
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
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Error = "No se pudo procesar ninguna lectura del archivo. Comprueba que el formato coincida con: FechaHora,Glucosa,Insulina";
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

        // Control de permisos sobre la eliminación
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
        // Si el médico quiere ver el gráfico de un paciente en concreto
        query = query.Where(l => l.Dispositivo != null && l.Dispositivo.PacienteId == pacienteId.Value);
        ViewBag.PacienteSeleccionado = await _context.Pacientes.FindAsync(pacienteId.Value);
    }

    // Orden cronológico ascendente para que la gráfica vaya de izquierda a derecha en el tiempo
    var lecturas = await query
        .OrderBy(l => l.FechaHora)
        .ToListAsync();

    return View(lecturas);
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
}