using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class etl_maintenance_configurationMap : EntityTypeConfiguration<etl_maintenance_configuration>
    {
        public etl_maintenance_configurationMap()
        {
            // Primary Key
            this.HasKey(t => t.configuration_id);

            // Properties
            this.Property(t => t.configuration_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.configuration_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.configuration_value)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("etl_maintenance_configuration", "mart");
            this.Property(t => t.configuration_id).HasColumnName("configuration_id");
            this.Property(t => t.configuration_name).HasColumnName("configuration_name");
            this.Property(t => t.configuration_value).HasColumnName("configuration_value");
        }
    }
}
