using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Api.Entities;

namespace TimeTracker.Api.Infrastructure.Persistence;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.GitHubUserId);
        builder.Property(x => x.GitHubLogin).HasMaxLength(100);
    }
}