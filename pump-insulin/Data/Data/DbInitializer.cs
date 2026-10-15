using Microsoft.EntityFrameworkCore;
using proyecto.Models;

namespace proyecto.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // aplica  migraciones pendientes al arrancar la app
        context.Database.Migrate();

        // DATOS DEMO
        
    }
}