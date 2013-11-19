using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_jobstepMap : EntityTypeConfiguration<etl_jobstep>
    {
        public etl_jobstepMap()
        {
            // Primary Key
            this.HasKey(t => t.jobstep_id);

            // Properties
            this.Property(t => t.jobstep_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.jobstep_name)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("etl_jobstep", "mart");
            this.Property(t => t.jobstep_id).HasColumnName("jobstep_id");
            this.Property(t => t.jobstep_name).HasColumnName("jobstep_name");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
