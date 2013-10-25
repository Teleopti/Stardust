using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_job_schedule_periodMap : EntityTypeConfiguration<etl_job_schedule_period>
    {
        public etl_job_schedule_periodMap()
        {
            // Primary Key
            this.HasKey(t => new { t.schedule_id, t.job_id });

            // Properties
            this.Property(t => t.schedule_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.job_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("etl_job_schedule_period", "mart");
            this.Property(t => t.schedule_id).HasColumnName("schedule_id");
            this.Property(t => t.job_id).HasColumnName("job_id");
            this.Property(t => t.relative_period_start).HasColumnName("relative_period_start");
            this.Property(t => t.relative_period_end).HasColumnName("relative_period_end");

            // Relationships
            this.HasRequired(t => t.etl_job)
                .WithMany(t => t.etl_job_schedule_period)
                .HasForeignKey(d => d.job_id);
            this.HasRequired(t => t.etl_job_schedule)
                .WithMany(t => t.etl_job_schedule_period)
                .HasForeignKey(d => d.schedule_id);

        }
    }
}
