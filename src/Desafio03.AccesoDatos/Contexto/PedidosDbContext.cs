using Desafio03.Entidades.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Desafio03.AccesoDatos.Contexto;

/// <summary>
/// Contexto de Entity Framework Core del sistema de pedidos.
/// </summary>
public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entidad =>
        {
            entidad.ToTable("Clientes");
            entidad.HasKey(c => c.Id);
            entidad.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
            entidad.Property(c => c.Apellido).IsRequired().HasMaxLength(50);
            entidad.Property(c => c.Email).IsRequired().HasMaxLength(100);

            // Regla de negocio reforzada en la base: email único.
            entidad.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Producto>(entidad =>
        {
            entidad.ToTable("Productos");
            entidad.HasKey(p => p.Id);
            entidad.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entidad.Property(p => p.Descripcion).IsRequired().HasMaxLength(500);
            entidad.Property(p => p.Precio).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Pedido>(entidad =>
        {
            entidad.ToTable("Pedidos");
            entidad.HasKey(p => p.Id);
            entidad.Property(p => p.FechaPedido).HasColumnType("datetime2");
            entidad.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");
            entidad.Property(p => p.Total).HasColumnType("decimal(18,2)");

            // Cliente 1 — N Pedidos
            entidad.HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto 1 — N Pedidos
            entidad.HasOne(p => p.Producto)
                .WithMany(p => p.Pedidos)
                .HasForeignKey(p => p.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
