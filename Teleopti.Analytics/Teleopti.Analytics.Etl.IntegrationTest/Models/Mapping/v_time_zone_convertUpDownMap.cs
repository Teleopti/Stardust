using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_time_zone_convertUpDownMap : EntityTypeConfiguration<v_time_zone_convertUpDown>
    {
        public v_time_zone_convertUpDownMap()
        {
            // Primary Key
            this.HasKey(t => new { t.time_zone_id, t.conversion });

            // Properties
            this.Property(t => t.time_zone_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.conversion)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("v_time_zone_convertUpDown", "mart");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.conversion).HasColumnName("conversion");
        }
    }
}
