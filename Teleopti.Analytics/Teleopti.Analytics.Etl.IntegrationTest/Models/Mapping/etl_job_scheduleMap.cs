using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_job_scheduleMap : EntityTypeConfiguration<etl_job_schedule>
    {
        public etl_job_scheduleMap()
        {
            // Primary Key
            this.HasKey(t => t.schedule_id);

            // Properties
            this.Property(t => t.schedule_name)
                .HasMaxLength(150);

            this.Property(t => t.etl_job_name)
                .IsRequired()
                .HasMaxLength(150);

            this.Property(t => t.description)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("etl_job_schedule", "mart");
            this.Property(t => t.schedule_id).HasColumnName("schedule_id");
            this.Property(t => t.schedule_name).HasColumnName("schedule_name");
            this.Property(t => t.enabled).HasColumnName("enabled");
            this.Property(t => t.schedule_type).HasColumnName("schedule_type");
            this.Property(t => t.occurs_daily_at).HasColumnName("occurs_daily_at");
            this.Property(t => t.occurs_every_minute).HasColumnName("occurs_every_minute");
            this.Property(t => t.recurring_starttime).HasColumnName("recurring_starttime");
            this.Property(t => t.recurring_endtime).HasColumnName("recurring_endtime");
            this.Property(t => t.etl_job_name).HasColumnName("etl_job_name");
            this.Property(t => t.etl_relative_period_start).HasColumnName("etl_relative_period_start");
            this.Property(t => t.etl_relative_period_end).HasColumnName("etl_relative_period_end");
            this.Property(t => t.etl_datasource_id).HasColumnName("etl_datasource_id");
            this.Property(t => t.description).HasColumnName("description");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
