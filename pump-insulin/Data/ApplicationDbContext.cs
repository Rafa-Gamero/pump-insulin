using Microsoft.EntityFrameworkCore;

namespace proyecto.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // TABLAS
    
}