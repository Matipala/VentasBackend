using Microsoft.EntityFrameworkCore;
using VentasBackend.Domain.Entities;

namespace VentasBackend.Infrastructure.Data;

public class VentasDbContext : DbContext
{
    public VentasDbContext(DbContextOptions<VentasDbContext> options) : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CuentaTicket> CuentasTickets => Set<CuentaTicket>();
    public DbSet<CuentaTicketItem> CuentasTicketItems => Set<CuentaTicketItem>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ConfiguracionVentas> Configuracion => Set<ConfiguracionVentas>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("ventas");

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas", "shared", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.IdEmpresa);
            entity.Property(e => e.IdEmpresa).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.Nombre);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);
            entity.Property(e => e.IdCliente).HasDefaultValueSql("gen_random_uuid()");
            entity.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.IdEmpresa);
            entity.HasIndex(e => new { e.IdEmpresa, e.Telefono });
        });

        modelBuilder.Entity<CuentaTicket>(entity =>
        {
            entity.HasKey(e => e.IdCuentaTicket);
            entity.Property(e => e.IdCuentaTicket).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Subtotal).HasColumnType("numeric(18,2)");
            entity.Property(e => e.Impuesto).HasColumnType("numeric(18,2)");
            entity.Property(e => e.Total).HasColumnType("numeric(18,2)");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(e => e.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.IdEmpresa, e.Numero }).IsUnique();
            entity.HasIndex(e => new { e.IdEmpresa, e.Estado });
            entity.HasIndex(e => e.FechaCreacion);
        });

        modelBuilder.Entity<CuentaTicketItem>(entity =>
        {
            entity.HasKey(e => e.IdCuentaTicketItem);
            entity.Property(e => e.IdCuentaTicketItem).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PrecioUnitario).HasColumnType("numeric(18,2)");
            entity.Property(e => e.Subtotal).HasColumnType("numeric(18,2)");

            entity.HasOne<CuentaTicket>()
                .WithMany()
                .HasForeignKey(e => e.IdCuentaTicket)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IdCuentaTicket);
            entity.HasIndex(e => e.IdProducto);
            entity.HasIndex(e => e.EstadoComanda);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago);
            entity.Property(e => e.IdPago).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Monto).HasColumnType("numeric(18,2)");
            entity.Property(e => e.FechaPago).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<CuentaTicket>()
                .WithMany()
                .HasForeignKey(e => e.IdCuentaTicket)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.IdEmpresa);
            entity.HasIndex(e => e.IdCuentaTicket);
            entity.HasIndex(e => e.FechaPago);
        });

        modelBuilder.Entity<ConfiguracionVentas>(entity =>
        {
            entity.ToTable("ConfiguracionVentas", "ventas");
            entity.HasKey(e => e.IdConfiguracion);
            entity.Property(e => e.IdConfiguracion).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PorcentajeImpuesto).HasColumnType("numeric(5,2)");
            entity.HasIndex(e => e.IdEmpresa).IsUnique();
        });
    }
}
