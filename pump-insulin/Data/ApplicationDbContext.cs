using Microsoft.EntityFrameworkCore;
using pumpinsulin.Models;

namespace pumpinsulin.Data; // <-- Asegúrate de que ponga pumpinsulin.Data

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Lectura> Lecturas => Set<Lectura>();
}