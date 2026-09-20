using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<WaterConnection> WaterConnections => Set<WaterConnection>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.ToTable("Residents");
            entity.HasKey(resident => resident.Id).HasName("PK_Residents");
            entity.Property(resident => resident.Id).HasColumnName("ResidentId");
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
            entity.HasKey(connection => connection.Id).HasName("PK_WaterConnections");
            entity.Property(connection => connection.Id).HasColumnName("WaterConnectionId");
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
            entity.HasKey(reading => reading.Id).HasName("PK_MeterReadings");
            entity.Property(reading => reading.Id).HasColumnName("MeterReadingId");
            entity.Property(reading => reading.PreviousReading).HasColumnType("decimal(18,2)");
            entity.Property(reading => reading.CurrentReading).HasColumnType("decimal(18,2)");
            entity.Property(reading => reading.Consumption).HasColumnType("decimal(18,2)").HasComputedColumnSql("[CurrentReading] - [PreviousReading]");
            entity.HasIndex(reading => new { reading.WaterConnectionId, reading.ReadingDate }).IsUnique();
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(reading => reading.WaterConnectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bills");
            entity.HasKey(bill => bill.Id).HasName("PK_Bills");
            entity.Property(bill => bill.Id).HasColumnName("BillId");
            entity.Property(bill => bill.UnitsConsumed).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.RatePerUnit).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.FixedCharge).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.TotalAmount).HasColumnType("decimal(18,2)").HasComputedColumnSql("[UnitsConsumed] * [RatePerUnit] + [FixedCharge] + [TaxAmount]");
            entity.Property(bill => bill.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(bill => new { bill.WaterConnectionId, bill.BillingPeriodStart, bill.BillingPeriodEnd }).IsUnique();
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(bill => bill.WaterConnectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id).HasName("PK_Payments");
            entity.Property(payment => payment.Id).HasColumnName("PaymentId");
            entity.Property(payment => payment.Amount).HasColumnType("decimal(18,2)");
            entity.Property(payment => payment.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(payment => payment.ReferenceNumber).HasMaxLength(60).IsRequired();
            entity.HasIndex(payment => payment.ReferenceNumber).IsUnique();
            entity.HasOne<Bill>().WithMany().HasForeignKey(payment => payment.BillId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.ToTable("ServiceRequests");
            entity.HasKey(request => request.Id).HasName("PK_ServiceRequests");
            entity.Property(request => request.Id).HasColumnName("ServiceRequestId");
            entity.Property(request => request.RequestType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(request => request.Description).HasMaxLength(1000).IsRequired();
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(request => request.StaffNotes).HasMaxLength(1000);
            entity.HasOne<Resident>().WithMany().HasForeignKey(request => request.ResidentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WaterConnection>().WithMany().HasForeignKey(request => request.WaterConnectionId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(notification => notification.Id).HasName("PK_Notifications");
            entity.Property(notification => notification.Id).HasColumnName("NotificationId");
            entity.Property(notification => notification.Title).HasMaxLength(160).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(1000).IsRequired();
            entity.Property(notification => notification.NotificationType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne<Resident>().WithMany().HasForeignKey(notification => notification.ResidentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
