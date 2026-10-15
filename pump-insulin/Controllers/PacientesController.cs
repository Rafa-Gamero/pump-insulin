using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

public class PacientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PacientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Pacientes
    public async Task<IActionResult> Index()
    {
        var pacientes = await _context.Pacientes
            .Include(p => p.Medico)
            .Include(p => p.Dispositivo)
            .ToListAsync();
        return View(pacientes);
    }

    // GET: Pacientes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var paciente = await _context.Pacientes
            .Include(p => p.Medico)
            .Include(p => p.Dispositivo)
                .ThenInclude(d => d!.Lecturas)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (paciente == null) return NotFound();

        return View(paciente);
    }

    // GET: Pacientes/Create
    public IActionResult Create()
    {
        CargarSelectMedicos();
        return View();
    }

    // POST: Pacientes/Create
    [HttpPost]
    public async Task<IActionResult> Create(Paciente paciente)
    {
        if (ModelState.IsValid)
        {
            _context.Add(paciente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        CargarSelectMedicos(paciente.MedicoId);
        return View(paciente);
    }

    // GET: Pacientes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        CargarSelectMedicos(paciente.MedicoId);
        return View(paciente);
    }

    // POST: Pacientes/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Paciente paciente)
    {
        if (id != paciente.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(paciente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pacientes.Any(e => e.Id == paciente.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        CargarSelectMedicos(paciente.MedicoId);
        return View(paciente);
    }

    // GET: Pacientes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var paciente = await _context.Pacientes
            .Include(p => p.Medico)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (paciente == null) return NotFound();

        return View(paciente);
    }

    // POST: Pacientes/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente != null)
        {
            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void CargarSelectMedicos(int? selectedId = null)
    {
        ViewBag.MedicoId = new SelectList(
            _context.Medicos.Select(m => new { m.Id, NombreCompleto = m.Nombre + " " + m.Apellidos }),
            "Id",
            "NombreCompleto",
            selectedId
        );
    }
}