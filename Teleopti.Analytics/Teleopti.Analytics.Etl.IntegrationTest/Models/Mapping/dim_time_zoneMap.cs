using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_time_zoneMap : EntityTypeConfiguration<dim_time_zone>
    {
        public dim_time_zoneMap()
        {
            // Primary Key
            this.HasKey(t => t.time_zone_id);

            // Properties
            this.Property(t => t.time_zone_code)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.time_zone_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_time_zone", "mart");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.time_zone_code).HasColumnName("time_zone_code");
            this.Property(t => t.time_zone_name).HasColumnName("time_zone_name");
            this.Property(t => t.default_zone).HasColumnName("default_zone");
            this.Property(t => t.utc_conversion).HasColumnName("utc_conversion");
            this.Property(t => t.utc_conversion_dst).HasColumnName("utc_conversion_dst");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.to_be_deleted).HasColumnName("to_be_deleted");
            this.Property(t => t.only_one_default_zone).HasColumnName("only_one_default_zone");
        }
    }
}
