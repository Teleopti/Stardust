using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_time_zoneMap : EntityTypeConfiguration<stg_time_zone>
    {
        public stg_time_zoneMap()
        {
            // Primary Key
            this.HasKey(t => t.time_zone_code);

            // Properties
            this.Property(t => t.time_zone_code)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.time_zone_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("stg_time_zone", "stage");
            this.Property(t => t.time_zone_code).HasColumnName("time_zone_code");
            this.Property(t => t.time_zone_name).HasColumnName("time_zone_name");
            this.Property(t => t.default_zone).HasColumnName("default_zone");
            this.Property(t => t.utc_conversion).HasColumnName("utc_conversion");
            this.Property(t => t.utc_conversion_dst).HasColumnName("utc_conversion_dst");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
