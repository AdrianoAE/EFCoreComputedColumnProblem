using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreComputedColumnProblem
{
    public interface IOwnedNavigationConfiguration<TEntity, TOwnedEntity>
       where TEntity : class
       where TOwnedEntity : class
    {
        void Configure(OwnedNavigationBuilder<TEntity, TOwnedEntity> buyerConfiguration);
    }
}
