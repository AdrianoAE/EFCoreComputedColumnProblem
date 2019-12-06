using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EFCoreComputedColumnProblem
{
    public static class ModelBuilderExtensions
    {
        public static EntityTypeBuilder<TEntity> ApplyOwnsOneConfiguration<TEntity, TOwnedEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Expression<Func<TEntity, TOwnedEntity>> navigationExpression,
            IOwnedNavigationConfiguration<TEntity, TOwnedEntity> configuration)
            where TEntity : class
            where TOwnedEntity : class
        {
            entityTypeBuilder.OwnsOne(navigationExpression, configuration.Configure);
            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ApplyOwnsManyConfiguration<TEntity, TOwnedEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Expression<Func<TEntity, IEnumerable<TOwnedEntity>>> navigationExpression,
            IOwnedNavigationConfiguration<TEntity, TOwnedEntity> configuration)
            where TEntity : class
            where TOwnedEntity : class
        {
            entityTypeBuilder.OwnsMany(navigationExpression, configuration.Configure);
            return entityTypeBuilder;
        }
    }
}
