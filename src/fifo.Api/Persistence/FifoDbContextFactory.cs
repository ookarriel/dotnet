using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace fifo.Api.Persistence;

/// <summary>
/// Permite ejecutar "dotnet ef migrations ..." sin necesidad de un SQL Server activo.
/// La cadena se toma de la configuración/entorno; el valor local solo se usa para
/// generar artefactos (migraciones o scripts), nunca contiene credenciales reales.
/// </summary>
public class FifoDbContextFactory : IDesignTimeDbContextFactory<FifoDbContext>
{
    public FifoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("MiConexion")
            ?? "Server=localhost,1433;Database=fifo;Integrated Security=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<FifoDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new FifoDbContext(options);
    }
}
