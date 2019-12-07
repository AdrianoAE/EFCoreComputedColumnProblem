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
            builder.OwnsOne(t => t.Percentage, b =>
            {
                b.Property(v => v.Value)
                    .HasColumnName("Percentage")
                    .IsRequired();

                b.Property(p => p.Multiplier)
                    .HasColumnType("decimal(3,2)")
                    .HasColumnName(nameof(TaxesPercentage.Multiplier))
                    .HasComputedColumnSql("(CONVERT([decimal](3,2),CONVERT([decimal](3,0),[Percentage])/(100.00)+(1))) PERSISTED");
            });
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class BloggingContext : DbContext
    {
        private readonly ILoggerFactory Logger
            = LoggerFactory.Create(c => c.AddConsole());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(Logger)
                .EnableSensitiveDataLogging()
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.Properties.Any(p => p.Metadata.Name == "IsDeleted") && e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsDeleted"] = true;
            }

            return base.SaveChanges();
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class Program
    {
        public static async Task Main()
        {
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

            using (var context = new BloggingContext())
            {
                var taxes = context.Set<Taxes>().Single();

                context.Remove(taxes);

                context.SaveChanges();
            }
        }
    }
}
