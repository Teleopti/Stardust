using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_jobstep_executionMap : EntityTypeConfiguration<etl_jobstep_execution>
    {
        public etl_jobstep_executionMap()
        {
            // Primary Key
            this.HasKey(t => t.jobstep_execution_id);

            // Properties
            this.Property(t => t.business_unit_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("etl_jobstep_execution", "mart");
            this.Property(t => t.jobstep_execution_id).HasColumnName("jobstep_execution_id");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.duration_s).HasColumnName("duration_s");
            this.Property(t => t.rows_affected).HasColumnName("rows_affected");
            this.Property(t => t.job_execution_id).HasColumnName("job_execution_id");
            this.Property(t => t.jobstep_error_id).HasColumnName("jobstep_error_id");
            this.Property(t => t.jobstep_id).HasColumnName("jobstep_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");

            // Relationships
            this.HasOptional(t => t.etl_job_execution)
                .WithMany(t => t.etl_jobstep_execution)
                .HasForeignKey(d => d.job_execution_id);
            this.HasOptional(t => t.etl_jobstep)
                .WithMany(t => t.etl_jobstep_execution)
                .HasForeignKey(d => d.jobstep_id);
            this.HasOptional(t => t.etl_jobstep_error)
                .WithMany(t => t.etl_jobstep_execution)
                .HasForeignKey(d => d.jobstep_error_id);

        }
    }
}
