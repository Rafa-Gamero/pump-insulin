using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pumpinsulin.Data;
using pumpinsulin.Models;

namespace pumpinsulin.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string userId = "";
        string userName = "";
        string role = model.Rol;

        if (model.Rol == "Medico")
        {
            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.Email == model.Email && m.Password == model.Password);
            if (medico == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas para médico.");
                return View(model);
            }
            userId = medico.Id.ToString();
            userName = $"{medico.Nombre} {medico.Apellidos}";
        }
        else
        {
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == model.Email && p.Password == model.Password);
            if (paciente == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas para paciente.");
                return View(model);
            }
            userId = paciente.Id.ToString();
            userName = $"{paciente.Nombre} {paciente.Apellidos}";
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        ViewBag.Medicos = new SelectList(_context.Medicos.Select(m => new { m.Id, Nombre = m.Nombre + " " + m.Apellidos }), "Id", "Nombre");
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegistroViewModel model)
    {
        if (model.Rol == "Medico")
        {
            var medico = new Medico
            {
                Nombre = model.Nombre,
                Apellidos = model.Apellidos,
                Email = model.Email,
                Password = model.Password,
                Especialidad = model.Especialidad ?? "Endocrinología",
                Telefono = model.Telefono ?? ""
            };
            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();
        }
        else
        {
            var paciente = new Paciente
            {
                Nombre = model.Nombre,
                Apellidos = model.Apellidos,
                Email = model.Email,
                Password = model.Password,
                Dni = model.Dni ?? "",
                FechaNacimiento = model.FechaNacimiento ?? DateTime.Today,
                MedicoId = model.MedicoId
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}