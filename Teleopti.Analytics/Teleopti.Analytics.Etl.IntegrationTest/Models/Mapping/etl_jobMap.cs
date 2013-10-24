using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_jobMap : EntityTypeConfiguration<etl_job>
    {
        public etl_jobMap()
        {
            // Primary Key
            this.HasKey(t => t.job_id);

            // Properties
            this.Property(t => t.job_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.job_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("etl_job", "mart");
            this.Property(t => t.job_id).HasColumnName("job_id");
            this.Property(t => t.job_name).HasColumnName("job_name");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
