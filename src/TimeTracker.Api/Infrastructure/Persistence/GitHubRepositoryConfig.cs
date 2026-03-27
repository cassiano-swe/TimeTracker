using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Api.Entities;

namespace TimeTracker.Api.Infrastructure.Persistence.Configurations;

public sealed class GitHubRepositoryConfig : IEntityTypeConfiguration<GitHubRepository>
{
    public void Configure(EntityTypeBuilder<GitHubRepository> b)
    {
        b.ToTable("github_repositories");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(300);

        b.Property(x => x.DefaultBranch)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => x.GitHubRepositoryId);

        b.HasIndex(x => new { x.WorkspaceId, x.FullName })
            .IsUnique();

        b.HasIndex(x => new { x.GitHubIntegrationId, x.GitHubRepositoryId })
            .IsUnique();
    }
}