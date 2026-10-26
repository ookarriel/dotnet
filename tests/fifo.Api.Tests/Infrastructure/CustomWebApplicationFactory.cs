using System.Text;
using fifo.Api.Configuration;
using fifo.Api.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace fifo.Api.Tests.Infrastructure;

/// <summary>
/// Ejecuta la API en memoria de proceso (TestServer) con una base aislada por prueba.
/// Modo predeterminado: SQLite relacional temporal (no requiere servidor).
/// Modo SQL Server real: FIFO_TESTS_SQLSERVER=1 y FIFO_TESTS_CONNECTION con una cadena válida.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@integration.local";
    public const string AdminPassword = "Admin-Integration-123!";
    private const string TestJwtKey = "integration-tests-signing-key-with-more-than-32-bytes";

    private readonly bool _useSqlServer = string.Equals(
        Environment.GetEnvironmentVariable("FIFO_TESTS_SQLSERVER"),
        "1",
        StringComparison.OrdinalIgnoreCase);

    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"fifo-integration-{Guid.NewGuid():N}.db");

    private readonly string _sqlServerDatabase = $"fifo_tests_{Guid.NewGuid():N}";
    private readonly string? _sqlServerConnectionString;

    public CustomWebApplicationFactory()
    {
        if (!_useSqlServer)
        {
            return;
        }

        var baseConnectionString = Environment.GetEnvironmentVariable("FIFO_TESTS_CONNECTION")
            ?? throw new InvalidOperationException(
                "FIFO_TESTS_SQLSERVER=1 requiere FIFO_TESTS_CONNECTION con una cadena de SQL Server válida.");

        _sqlServerConnectionString = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = _sqlServerDatabase
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // En modo SQL Server esta es la cadena real; en modo SQLite se sustituye el DbContext.
                ["ConnectionStrings:DefaultConnection"] = _sqlServerConnectionString
                    ?? "Server=localhost,1433;Database=fifo_tests;Integrated Security=true;TrustServerCertificate=True",
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = "fifoIntegrationTests",
                ["Jwt:Audience"] = "fifoIntegrationClients",
                ["SeedData:AdminEmail"] = AdminEmail,
                ["SeedData:AdminPassword"] = AdminPassword
            });
        });

        // Estas sustituciones se aplican después de Program.cs y aíslan cada prueba.
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<FifoDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.AddDbContext<FifoDbContext>(options =>
            {
                if (_useSqlServer)
                {
                    options.UseSqlServer(_sqlServerConnectionString!);
                }
                else
                {
                    options.UseSqlite($"Data Source={_databasePath}");
                }
            });

            services.Configure<JwtOptions>(options =>
            {
                options.Key = TestJwtKey;
                options.Issuer = "fifoIntegrationTests";
                options.Audience = "fifoIntegrationClients";
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidIssuer = "fifoIntegrationTests";
                options.TokenValidationParameters.ValidAudience = "fifoIntegrationClients";
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_useSqlServer)
        {
            DropSqlServerDatabase();
            return;
        }

        foreach (var path in new[] { _databasePath, $"{_databasePath}-shm", $"{_databasePath}-wal" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private void DropSqlServerDatabase()
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(_sqlServerConnectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                $"ALTER DATABASE [{_sqlServerDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                $"DROP DATABASE [{_sqlServerDatabase}];";
            command.ExecuteNonQuery();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"No se pudo eliminar {_sqlServerDatabase}: {exception.Message}");
        }
    }
}
