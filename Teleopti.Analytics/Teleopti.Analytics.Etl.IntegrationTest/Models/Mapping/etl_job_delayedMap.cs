using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_job_delayedMap : EntityTypeConfiguration<etl_job_delayed>
    {
        public etl_job_delayedMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.stored_procedured)
                .IsRequired()
                .HasMaxLength(300);

            this.Property(t => t.parameter_string)
                .IsRequired()
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("etl_job_delayed", "mart");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.stored_procedured).HasColumnName("stored_procedured");
            this.Property(t => t.parameter_string).HasColumnName("parameter_string");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.execute_date).HasColumnName("execute_date");
        }
    }
}
