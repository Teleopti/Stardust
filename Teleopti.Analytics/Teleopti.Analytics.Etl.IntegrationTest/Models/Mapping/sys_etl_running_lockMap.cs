using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class sys_etl_running_lockMap : EntityTypeConfiguration<sys_etl_running_lock>
    {
        public sys_etl_running_lockMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.computer_name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.job_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("sys_etl_running_lock", "mart");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.computer_name).HasColumnName("computer_name");
            this.Property(t => t.start_time).HasColumnName("start_time");
            this.Property(t => t.job_name).HasColumnName("job_name");
            this.Property(t => t.is_started_by_service).HasColumnName("is_started_by_service");
            this.Property(t => t.lock_until).HasColumnName("lock_until");
        }
    }
}
