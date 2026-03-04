using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Api.Entities;

namespace Fido.Time.Api.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceMemberConfig : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> b)
    {
        b.ToTable("workspace_members");
        b.HasKey(x => new { x.WorkspaceId, x.UserId });
        b.Property(x => x.Role).IsRequired();
        b.HasIndex(x => x.UserId);
    }
}