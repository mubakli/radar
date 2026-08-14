using Microsoft.EntityFrameworkCore;

namespace Radar.Api.Data;

public sealed class RadarDbContext(DbContextOptions<RadarDbContext> options) : DbContext(options)
{
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<SourceItem> SourceItems => Set<SourceItem>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StorySourceItem> StorySourceItems => Set<StorySourceItem>();
    public DbSet<FetchAttempt> FetchAttempts => Set<FetchAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Locator).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Locator).HasMaxLength(2_000).IsRequired();
            entity.Property(x => x.Enabled).HasDefaultValue(true).ValueGeneratedNever();
        });

        modelBuilder.Entity<SourceItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SourceId, x.CanonicalLocator }).IsUnique();
            entity.Property(x => x.CanonicalLocator).HasMaxLength(2_000).IsRequired();
            entity.Property(x => x.Url).HasMaxLength(2_000);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Author).HasMaxLength(500);
            entity.Property(x => x.Summary).HasMaxLength(10_000);
            entity.Property(x => x.RawContent).IsRequired();
            entity.HasOne(x => x.Source).WithMany(x => x.Items).HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FetchAttempt>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FailureCategory).HasMaxLength(100);
            entity.Property(x => x.Message).HasMaxLength(1_000);
            entity.HasOne(x => x.Source).WithMany(x => x.FetchAttempts).HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.SourceId, x.AttemptedAt });
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Summary).IsRequired();
        });

        modelBuilder.Entity<StorySourceItem>(entity =>
        {
            entity.HasKey(x => new { x.StoryId, x.SourceItemId });
            entity.Property(x => x.MembershipMethod).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MembershipReason).HasMaxLength(1_000).IsRequired();
            entity.HasOne(x => x.Story).WithMany(x => x.SourceItems).HasForeignKey(x => x.StoryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SourceItem).WithMany(x => x.StoryMemberships).HasForeignKey(x => x.SourceItemId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
