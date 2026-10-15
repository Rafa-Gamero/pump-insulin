using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

public class MedicosController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Medicos
    public async Task<IActionResult> Index()
    {
        var medicos = await _context.Medicos
            .Include(m => m.Pacientes)
            .ToListAsync();
        return View(medicos);
    }

    // GET: Medicos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var medico = await _context.Medicos
            .Include(m => m.Pacientes)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico == null) return NotFound();

        return View(medico);
    }

    // GET: Medicos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Medicos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medico medico)
    {
        if (ModelState.IsValid)
        {
            _context.Add(medico);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(medico);
    }

    // GET: Medicos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound();

        return View(medico);
    }

    // POST: Medicos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Medico medico)
    {
        if (id != medico.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(medico);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Medicos.Any(e => e.Id == medico.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(medico);
    }

    // GET: Medicos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var medico = await _context.Medicos
            .Include(m => m.Pacientes)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico == null) return NotFound();

        return View(medico);
    }

    // POST: Medicos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico != null)
        {
            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}