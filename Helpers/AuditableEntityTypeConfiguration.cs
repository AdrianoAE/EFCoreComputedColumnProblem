using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EFCoreComputedColumnProblem
{
    public abstract class AuditableEntityTypeConfiguration<T> : IEntityTypeConfiguration<T>
        where T : class
    {
        public virtual void Configure(EntityTypeBuilder<T> builderConfiguration)
        {
            builderConfiguration.Property<DateTime>("Created")
                .HasDefaultValueSql("SYSDATETIME()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builderConfiguration.Property<int>("CreatedBy")
                .IsRequired();

            builderConfiguration.Property<DateTime>("LastModified")
                .HasDefaultValueSql("SYSDATETIME()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builderConfiguration.Property<int>("LastModifiedBy")
                .IsRequired();

            builderConfiguration.Property<bool>("IsDeleted")
                .HasDefaultValue(false)
                .IsRequired();

            //─────────────────────────────────── Query Filters ───────────────────────────────────

            builderConfiguration.HasQueryFilter(b => EF.Property<bool>(b, "IsDeleted") == false);
        }
    }
}
