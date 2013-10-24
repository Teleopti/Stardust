using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class service_level_calculationMap : EntityTypeConfiguration<service_level_calculation>
    {
        public service_level_calculationMap()
        {
            // Primary Key
            this.HasKey(t => t.service_level_id);

            // Properties
            this.Property(t => t.service_level_name)
                .HasMaxLength(100);

            this.Property(t => t.resource_key)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("service_level_calculation", "mart");
            this.Property(t => t.service_level_id).HasColumnName("service_level_id");
            this.Property(t => t.service_level_name).HasColumnName("service_level_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
