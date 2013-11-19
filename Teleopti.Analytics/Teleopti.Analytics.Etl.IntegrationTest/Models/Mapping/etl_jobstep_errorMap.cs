using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_jobstep_errorMap : EntityTypeConfiguration<etl_jobstep_error>
    {
        public etl_jobstep_errorMap()
        {
            // Primary Key
            this.HasKey(t => t.jobstep_error_id);

            // Properties
            // Table & Column Mappings
            this.ToTable("etl_jobstep_error", "mart");
            this.Property(t => t.jobstep_error_id).HasColumnName("jobstep_error_id");
            this.Property(t => t.error_exception_message).HasColumnName("error_exception_message");
            this.Property(t => t.error_exception_stacktrace).HasColumnName("error_exception_stacktrace");
            this.Property(t => t.inner_error_exception_message).HasColumnName("inner_error_exception_message");
            this.Property(t => t.inner_error_exception_stacktrace).HasColumnName("inner_error_exception_stacktrace");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
