using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_time_zone_bridgeMap : EntityTypeConfiguration<stg_time_zone_bridge>
    {
        public stg_time_zone_bridgeMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date, t.interval_id, t.time_zone_code });

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.time_zone_code)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_time_zone_bridge", "stage");
            this.Property(t => t.date).HasColumnName("date");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.time_zone_code).HasColumnName("time_zone_code");
            this.Property(t => t.local_date).HasColumnName("local_date");
            this.Property(t => t.local_interval_id).HasColumnName("local_interval_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
