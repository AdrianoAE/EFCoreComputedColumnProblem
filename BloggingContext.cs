using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EFCoreComputedColumnProblem
{
    public class PercentageConfiguration<TEntity> : IOwnedNavigationConfiguration<TEntity, TaxesPercentage>
           where TEntity : class
    {
        public void Configure(OwnedNavigationBuilder<TEntity, TaxesPercentage> percentageConfiguration)
        {
            percentageConfiguration.Property(v => v.Value)
                .HasColumnName("Percentage")
                .IsRequired();

            percentageConfiguration.Property(v => v.Multiplier)
                .HasColumnType("decimal(3,2)")
                .HasColumnName(nameof(TaxesPercentage.Multiplier))
                .HasComputedColumnSql("(CONVERT([decimal](3,2),CONVERT([decimal](3,0),[Percentage])/(100.00)+(1))) PERSISTED");
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    class IvaEntityConfiguration : AuditableEntityTypeConfiguration<Taxes>
    {
        public override void Configure(EntityTypeBuilder<Taxes> ivaConfiguration)
        {
            ivaConfiguration.ToTable("Iva");

            //─────────────────────────────────────────────────────────────────────────────────────

            ivaConfiguration.ApplyOwnsOneConfiguration(i => i.Percentage, new PercentageConfiguration<Taxes>());

            //─────────────────────────────────────────────────────────────────────────────────────

            base.Configure(ivaConfiguration);
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class BloggingContext : DbContext
    {
        private readonly ILoggerFactory Logger
            = LoggerFactory.Create(c => c.AddConsole());

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(Logger)
                .EnableSensitiveDataLogging()
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public override int SaveChanges()
        {
            var currentTime = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Properties.Any(p => p.Metadata.Name == "Created")))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("Created").CurrentValue = currentTime;
                }

                entry.Property("LastModified").CurrentValue = currentTime;
            }

            UpdateSoftDeleteStatuses();

            return base.SaveChanges();
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private void UpdateSoftDeleteStatuses()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.Properties.Any(p => p.Metadata.Name == "IsDeleted") && e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsDeleted"] = true;
            }
        }
    }
}
