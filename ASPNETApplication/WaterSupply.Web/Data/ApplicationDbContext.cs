using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Web.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<WaterConnection> WaterConnections => Set<WaterConnection>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.ToTable("Residents");
            entity.HasKey(resident => resident.ResidentId).HasName("PK_Residents");
            entity.Property(resident => resident.ResidentId).ValueGeneratedOnAdd();
            entity.Property(resident => resident.FullName).HasMaxLength(120).IsRequired();
            entity.Property(resident => resident.Email).HasMaxLength(255).IsRequired();
            entity.Property(resident => resident.Phone).HasMaxLength(30).IsRequired();
            entity.Property(resident => resident.Address).HasMaxLength(300).IsRequired();
            entity.Property(resident => resident.IdentityUserId).HasMaxLength(450);
            entity.HasIndex(resident => resident.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
        });

        modelBuilder.Entity<WaterConnection>(entity =>
        {
            entity.ToTable("WaterConnections");
            entity.HasKey(connection => connection.WaterConnectionId).HasName("PK_WaterConnections");
            entity.Property(connection => connection.WaterConnectionId).ValueGeneratedOnAdd();
            entity.Property(connection => connection.ConnectionNumber).HasMaxLength(40).IsRequired();
            entity.Property(connection => connection.ConnectionType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(connection => connection.MeterNumber).HasMaxLength(40).IsRequired();
            entity.Property(connection => connection.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(connection => connection.ConnectionNumber).IsUnique();
            entity.HasOne<Resident>().WithMany().HasForeignKey(connection => connection.ResidentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.ToTable("MeterReadings");
            entity.HasKey(reading => reading.MeterReadingId).HasName("PK_MeterReadings");
            entity.Property(reading => reading.MeterReadingId).ValueGeneratedOnAdd();
            entity.Property(reading => reading.PreviousReading).HasColumnType("decimal(18,2)");
            entity.Property(reading => reading.CurrentReading).HasColumnType("decimal(18,2)");
            entity.Property(reading => reading.Consumption).HasColumnType("decimal(18,2)");
            entity.HasIndex(reading => new { reading.WaterConnectionId, reading.ReadingDate }).IsUnique();
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(reading => reading.WaterConnectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bills");
            entity.HasKey(bill => bill.BillId).HasName("PK_Bills");
            entity.Property(bill => bill.BillId).ValueGeneratedOnAdd();
            entity.Property(bill => bill.UnitsConsumed).HasColumnType("int");
            entity.Property(bill => bill.RatePerUnit).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(bill => new { bill.WaterConnectionId, bill.BillDate }).IsUnique();
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(bill => bill.WaterConnectionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MeterReading>().WithMany().HasForeignKey(bill => bill.MeterReadingId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.ToTable("ServiceRequests");
            entity.HasKey(request => request.ServiceRequestId).HasName("PK_ServiceRequests");
            entity.Property(request => request.ServiceRequestId).ValueGeneratedOnAdd();
            entity.Property(request => request.RequestType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(request => request.Description).HasMaxLength(1000).IsRequired();
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(request => request.StaffNotes).HasMaxLength(1000);
            entity.HasOne<Resident>().WithMany().HasForeignKey(request => request.ResidentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(request => request.WaterConnectionId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
