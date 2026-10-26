using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using fifo.Api.Configuration;
using fifo.Api.Middleware;
using fifo.Api.Persistence;
using fifo.Api.Repositories;
using fifo.Api.Services;
using fifo.Api.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// La API usa exclusivamente SQL Server. La cadena se obtiene de configuración
// (variables de entorno o user-secrets); nunca se almacenan credenciales en el código.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("MiConexion");

if (string.IsNullOrWhiteSpace(connectionString))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException(
            "Configure ConnectionStrings__DefaultConnection con la cadena de SQL Server.");
    }

    // Valor local sin credenciales: permite generar migraciones/scripts sin servidor.
    // Para ejecutar la API se debe configurar la cadena real mediante el entorno.
    connectionString = "Server=localhost,1433;Database=fifo;Integrated Security=true;TrustServerCertificate=True";
}

builder.Services.AddDbContext<FifoDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3)));

var jwtKey = builder.Configuration["Jwt:Key"];
var usesEphemeralDevelopmentKey = false;
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("Configure Jwt__Key mediante una variable de entorno.");
    }

    // Solo facilita el arranque local: cada reinicio invalida los tokens anteriores.
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    usesEphemeralDevelopmentKey = true;
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes.");
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Key = jwtKey;
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "fifoApi";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "fifoClients";
    options.ExpirationMinutes = builder.Configuration.GetValue("Jwt:ExpirationMinutes", 60);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "fifoApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "fifoClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// EF Core, repositorios y servicios se comparten por solicitud (Scoped).
// TokenService es Singleton porque solo depende de configuración inmutable.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese: Bearer {su token JWT}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (usesEphemeralDevelopmentKey)
{
    app.Logger.LogWarning(
        "Jwt:Key no está configurada; se generó una clave efímera para desarrollo. Configure Jwt__Key para conservar los tokens entre reinicios.");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"fifo API {description.GroupName}");
    }
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FifoDbContext>();
    var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true);

    if (applyMigrations)
    {
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            // Crea la base si no existe y aplica las migraciones SQL Server pendientes.
            await context.Database.MigrateAsync();
        }
        else
        {
            // Solo para proveedores alternativos usados por las pruebas locales.
            await context.Database.EnsureCreatedAsync();
        }
    }

    await DataSeeder.SeedAsync(context, app.Configuration);
}

await app.RunAsync();

// Requerido por WebApplicationFactory en las pruebas de integración.
public partial class Program;
