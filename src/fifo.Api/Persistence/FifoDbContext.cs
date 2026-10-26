using fifo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Persistence;

public class FifoDbContext(DbContextOptions<FifoDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.CodigoSku).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.HasIndex(e => e.CodigoSku).IsUnique();
            entity.HasOne(e => e.Categoria)
                .WithMany(e => e.Productos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.ToTable("Lotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoLote).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PrecioCompra).HasPrecision(18, 2);
            entity.HasIndex(e => e.CodigoLote).IsUnique();
            entity.HasIndex(e => new { e.ProductoId, e.FechaIngreso });
            entity.HasOne(e => e.Producto)
                .WithMany(e => e.Lotes)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.ToTable("Movimientos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(200);
            entity.Property(e => e.UsuarioResponsable).HasMaxLength(150);
            entity.HasIndex(e => new { e.ProductoId, e.Fecha });
            entity.HasOne(e => e.Lote)
                .WithMany(e => e.Movimientos)
                .HasForeignKey(e => e.LoteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Producto)
                .WithMany(e => e.Movimientos)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
