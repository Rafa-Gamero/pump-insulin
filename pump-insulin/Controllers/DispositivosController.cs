using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

public class DispositivosController : Controller
{
    private readonly ApplicationDbContext _context;

    public DispositivosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Dispositivos
    public async Task<IActionResult> Index()
    {
        var dispositivos = await _context.Dispositivos
            .Include(d => d.Paciente)
            .Include(d => d.Lecturas)
            .ToListAsync();
        return View(dispositivos);
    }

    // GET: Dispositivos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var dispositivo = await _context.Dispositivos
            .Include(d => d.Paciente)
            .Include(d => d.Lecturas)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dispositivo == null) return NotFound();

        return View(dispositivo);
    }

    // GET: Dispositivos/Create
    public IActionResult Create()
    {
        CargarSelectPacientes();
        return View();
    }

    // POST: Dispositivos/Create
    [HttpPost]
    public async Task<IActionResult> Create(Dispositivo dispositivo)
    {
        if (ModelState.IsValid)
        {
            _context.Add(dispositivo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        CargarSelectPacientes(dispositivo.PacienteId);
        return View(dispositivo);
    }

    // GET: Dispositivos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var dispositivo = await _context.Dispositivos.FindAsync(id);
        if (dispositivo == null) return NotFound();

        CargarSelectPacientes(dispositivo.PacienteId);
        return View(dispositivo);
    }

    // POST: Dispositivos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Dispositivo dispositivo)
    {
        if (id != dispositivo.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(dispositivo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Dispositivos.Any(e => e.Id == dispositivo.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        CargarSelectPacientes(dispositivo.PacienteId);
        return View(dispositivo);
    }

    // GET: Dispositivos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var dispositivo = await _context.Dispositivos
            .Include(d => d.Paciente)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dispositivo == null) return NotFound();

        return View(dispositivo);
    }

    // POST: Dispositivos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dispositivo = await _context.Dispositivos.FindAsync(id);
        if (dispositivo != null)
        {
            _context.Dispositivos.Remove(dispositivo);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void CargarSelectPacientes(int? selectedId = null)
    {
        ViewBag.PacienteId = new SelectList(
            _context.Pacientes.Select(p => new { p.Id, NombreCompleto = p.Nombre + " " + p.Apellidos + " (" + p.Dni + ")" }),
            "Id",
            "NombreCompleto",
            selectedId
        );
    }
}