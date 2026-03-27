using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Api.Entities;

namespace TimeTracker.Api.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceInviteConfig : IEntityTypeConfiguration<WorkspaceInvite>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvite> b)
    {
        b.ToTable("workspace_invites");

        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        b.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20);

        b.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        b.Property(x => x.ExpiresAt)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => x.Token)
            .IsUnique();

        b.HasIndex(x => new { x.WorkspaceId, x.Status });

        b.HasIndex(x => new { x.WorkspaceId, x.Email });
    }
}