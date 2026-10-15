using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

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
        var lecturas = await _context.Lecturas
            .Include(l => l.Dispositivo)
                .ThenInclude(d => d!.Paciente)
            .OrderByDescending(l => l.FechaHora)
            .ToListAsync();
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

        return View(lectura);
    }

    // GET: Lecturas/Create
    public IActionResult Create()
    {
        CargarSelectDispositivos();
        return View(new Lectura { FechaHora = DateTime.Now });
    }

    // POST: Lecturas/Create
    [HttpPost]
    public async Task<IActionResult> Create(Lectura lectura)
    {
        if (ModelState.IsValid)
        {
            _context.Add(lectura);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        CargarSelectDispositivos(lectura.DispositivoId);
        return View(lectura);
    }

    // GET: Lecturas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var lectura = await _context.Lecturas.FindAsync(id);
        if (lectura == null) return NotFound();

        CargarSelectDispositivos(lectura.DispositivoId);
        return View(lectura);
    }

    // POST: Lecturas/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Lectura lectura)
    {
        if (id != lectura.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(lectura);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Lecturas.Any(e => e.Id == lectura.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        CargarSelectDispositivos(lectura.DispositivoId);
        return View(lectura);
    }

    // GET: Lecturas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var lectura = await _context.Lecturas
            .Include(l => l.Dispositivo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (lectura == null) return NotFound();

        return View(lectura);
    }

    // POST: Lecturas/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lectura = await _context.Lecturas.FindAsync(id);
        if (lectura != null)
        {
            _context.Lecturas.Remove(lectura);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void CargarSelectDispositivos(int? selectedId = null)
    {
        ViewBag.DispositivoId = new SelectList(
            _context.Dispositivos.Select(d => new {
                d.Id,
                Descripcion = d.Modelo + " - " + d.NumeroSerie + (d.Paciente != null ? " (" + d.Paciente.Nombre + ")" : "")
            }),
            "Id",
            "Descripcion",
            selectedId
        );
    }
}