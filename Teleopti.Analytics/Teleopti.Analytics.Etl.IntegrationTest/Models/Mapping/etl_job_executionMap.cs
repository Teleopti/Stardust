using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_job_executionMap : EntityTypeConfiguration<etl_job_execution>
    {
        public etl_job_executionMap()
        {
            // Primary Key
            this.HasKey(t => t.job_execution_id);

            // Properties
            this.Property(t => t.business_unit_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("etl_job_execution", "mart");
            this.Property(t => t.job_execution_id).HasColumnName("job_execution_id");
            this.Property(t => t.job_id).HasColumnName("job_id");
            this.Property(t => t.schedule_id).HasColumnName("schedule_id");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.job_start_time).HasColumnName("job_start_time");
            this.Property(t => t.job_end_time).HasColumnName("job_end_time");
            this.Property(t => t.duration_s).HasColumnName("duration_s");
            this.Property(t => t.affected_rows).HasColumnName("affected_rows");
            this.Property(t => t.job_execution_success).HasColumnName("job_execution_success");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");

            // Relationships
            this.HasOptional(t => t.etl_job)
                .WithMany(t => t.etl_job_execution)
                .HasForeignKey(d => d.job_id);
            this.HasOptional(t => t.etl_job_schedule)
                .WithMany(t => t.etl_job_execution)
                .HasForeignKey(d => d.schedule_id);

        }
    }
}
