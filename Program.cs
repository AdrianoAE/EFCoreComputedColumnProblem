using System;
using System.Linq;
using System.Threading.Tasks;

namespace EFCoreComputedColumnProblem
{
    public class Taxes
    {
        public int Id { get; set; }
        public TaxesPercentage Percentage { get; set; }
    }

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

            using (var context = new BloggingContext())
            {
                var taxes = context.Set<Taxes>().Single();

                context.Remove(taxes);

                context.SaveChanges();
            }
        }
    }
}
