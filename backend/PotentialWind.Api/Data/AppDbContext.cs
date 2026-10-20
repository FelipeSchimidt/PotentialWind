using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Models;

namespace PotentialWind.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WindTurbine> WindTurbines => Set<WindTurbine>();
    public DbSet<WindFarm> WindFarms => Set<WindFarm>();
    public DbSet<WindMeasurement> WindMeasurements => Set<WindMeasurement>();
    public DbSet<WindSimulationResult> WindSimulationResults => Set<WindSimulationResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WindTurbine
        modelBuilder.Entity<WindTurbine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Manufacturer).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModelName).HasMaxLength(100).IsRequired();
        });

        // WindFarm
        modelBuilder.Entity<WindFarm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(150);

            entity.Property(e => e.EnergyPriceMWh).HasPrecision(18, 2);
            entity.Property(e => e.CapexPerMW).HasPrecision(18, 2);

            entity.HasOne(e => e.SelectedTurbine)
                  .WithMany()
                  .HasForeignKey(e => e.SelectedTurbineId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Measurements)
                  .WithOne(m => m.WindFarm)
                  .HasForeignKey(m => m.WindFarmId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.SimulationResults)
                  .WithOne(s => s.WindFarm)
                  .HasForeignKey(s => s.WindFarmId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WindSimulationResult
        modelBuilder.Entity<WindSimulationResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnnualRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalCapex).HasPrecision(18, 2);
            entity.Property(e => e.AnnualOpex).HasPrecision(18, 2);
            entity.Property(e => e.LcoePerMWh).HasPrecision(18, 2);
            entity.Property(e => e.Npv).HasPrecision(18, 2);
        });

        // WindMeasurement
        modelBuilder.Entity<WindMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WindFarmId, e.Timestamp });
        });
    }
}
