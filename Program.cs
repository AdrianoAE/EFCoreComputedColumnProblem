using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EFCoreComputedColumnProblem
{
    public class TaxesPercentage
    {
        public int Value { get; set; }
        public decimal Multiplier { get; set; }

        public TaxesPercentage(int value)
        {
            Value = value;
        }
    }

    public class Taxes
    {
        public int Id { get; set; }
        public TaxesPercentage Percentage { get; set; }
        public bool IsDeleted { get; set; }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    class IvaEntityConfiguration : IEntityTypeConfiguration<Taxes>
    {
        public void Configure(EntityTypeBuilder<Taxes> builder)
        {
            builder.OwnsOne(t => t.Percentage, tp =>
            {
                tp.Property(p => p.Value)
                    .HasColumnName(nameof(Taxes.Percentage))
                    .IsRequired();

                tp.Property(p => p.Multiplier)
                    .HasColumnType("decimal(3,2)")
                    .HasColumnName(nameof(TaxesPercentage.Multiplier))
                    .HasComputedColumnSql("(CONVERT([decimal](3,2),CONVERT([decimal](3,0),[Percentage])/(100.00)+(1))) PERSISTED");
            });
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class BloggingContext : DbContext
    {
        #region --- Configurations ---
        private readonly ILoggerFactory Logger
            = LoggerFactory.Create(c => c.AddConsole());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(Logger)
                .EnableSensitiveDataLogging()
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");
        #endregion

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.Properties.Any(p => p.Metadata.Name == "IsDeleted") && e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
            }

            return base.SaveChanges();
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class Program
    {
        public static async Task Main()
        {
            #region --- Setup ---
            using (var context = new BloggingContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Add(new Taxes
                {
                    Percentage = new TaxesPercentage(10)
                });

                context.SaveChanges();
            }

            Console.Clear();
            #endregion

            using (var context = new BloggingContext())
            {
                var taxes = context.Set<Taxes>().Single();

                context.Remove(taxes);

                context.SaveChanges();
            }
        }
    }
}
