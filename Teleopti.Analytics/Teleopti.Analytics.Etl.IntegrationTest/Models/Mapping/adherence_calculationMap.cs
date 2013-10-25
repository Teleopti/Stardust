using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class adherence_calculationMap : EntityTypeConfiguration<adherence_calculation>
    {
        public adherence_calculationMap()
        {
            // Primary Key
            this.HasKey(t => t.adherence_id);

            // Properties
            this.Property(t => t.adherence_name)
                .HasMaxLength(100);

            this.Property(t => t.resource_key)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("adherence_calculation", "mart");
            this.Property(t => t.adherence_id).HasColumnName("adherence_id");
            this.Property(t => t.adherence_name).HasColumnName("adherence_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
