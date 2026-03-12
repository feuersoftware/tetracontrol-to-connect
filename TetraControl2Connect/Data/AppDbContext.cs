using Microsoft.EntityFrameworkCore;

namespace FeuerSoftware.TetraControl2Connect.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ProgramSettingsEntity> ProgramSettings { get; set; } = null!;
        public DbSet<TetraControlSettingsEntity> TetraControlSettings { get; set; } = null!;
        public DbSet<StatusSettingsEntity> StatusSettings { get; set; } = null!;
        public DbSet<PatternSettingsEntity> PatternSettings { get; set; } = null!;
        public DbSet<AdditionalPatternEntity> AdditionalPatterns { get; set; } = null!;
        public DbSet<SeveritySettingsEntity> SeveritySettings { get; set; } = null!;
        public DbSet<SeverityTranslationEntity> SeverityTranslations { get; set; } = null!;
        public DbSet<SirenCalloutSettingsEntity> SirenCalloutSettings { get; set; } = null!;
        public DbSet<SirenCodeTranslationEntity> SirenCodeTranslations { get; set; } = null!;
        public DbSet<SirenStatusSettingsEntity> SirenStatusSettings { get; set; } = null!;
        public DbSet<FailureTranslationEntity> FailureTranslations { get; set; } = null!;
        public DbSet<SiteEntity> Sites { get; set; } = null!;
        public DbSet<SubnetAddressEntity> SubnetAddresses { get; set; } = null!;
        public DbSet<SirenEntity> Sirens { get; set; } = null!;
        public DbSet<SettingsBackupEntity> SettingsBackups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SiteEntity → SubnetAddressEntity (cascade delete)
            modelBuilder.Entity<SiteEntity>()
                .HasMany(s => s.SubnetAddresses)
                .WithOne()
                .HasForeignKey(sa => sa.SiteId)
                .OnDelete(DeleteBehavior.Cascade);

            // SiteEntity → SirenEntity (cascade delete)
            modelBuilder.Entity<SiteEntity>()
                .HasMany(s => s.Sirens)
                .WithOne()
                .HasForeignKey(si => si.SiteId)
                .OnDelete(DeleteBehavior.Cascade);

            // PatternSettingsEntity → AdditionalPatternEntity (cascade delete)
            modelBuilder.Entity<PatternSettingsEntity>()
                .HasMany(p => p.AdditionalProperties)
                .WithOne()
                .HasForeignKey(ap => ap.PatternSettingsId)
                .OnDelete(DeleteBehavior.Cascade);

            // SeveritySettingsEntity → SeverityTranslationEntity (cascade delete)
            modelBuilder.Entity<SeveritySettingsEntity>()
                .HasMany(s => s.SeverityTranslations)
                .WithOne()
                .HasForeignKey(st => st.SeveritySettingsId)
                .OnDelete(DeleteBehavior.Cascade);

            // SirenCalloutSettingsEntity → SirenCodeTranslationEntity (cascade delete)
            modelBuilder.Entity<SirenCalloutSettingsEntity>()
                .HasMany(s => s.SirenCodeTranslations)
                .WithOne()
                .HasForeignKey(sct => sct.SirenCalloutSettingsId)
                .OnDelete(DeleteBehavior.Cascade);

            // SirenStatusSettingsEntity → FailureTranslationEntity (cascade delete)
            modelBuilder.Entity<SirenStatusSettingsEntity>()
                .HasMany(s => s.FailureTranslations)
                .WithOne()
                .HasForeignKey(ft => ft.SirenStatusSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
