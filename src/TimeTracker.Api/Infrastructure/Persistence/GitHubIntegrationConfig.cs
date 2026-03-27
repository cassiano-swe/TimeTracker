using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Api.Entities;

namespace TimeTracker.Api.Infrastructure.Persistence.Configurations;

public sealed class GitHubIntegrationConfig : IEntityTypeConfiguration<GitHubIntegration>
{
    public void Configure(EntityTypeBuilder<GitHubIntegration> b)
    {
        b.ToTable("github_integrations");

        b.HasKey(x => x.Id);

        b.Property(x => x.AccountLogin)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.AccountType)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => x.WorkspaceId)
            .IsUnique();

        b.HasIndex(x => x.InstallationId)
            .IsUnique();
    }
}