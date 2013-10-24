using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class bridge_time_zoneMap : EntityTypeConfiguration<bridge_time_zone>
    {
        public bridge_time_zoneMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.time_zone_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.time_zone_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("bridge_time_zone", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.local_date_id).HasColumnName("local_date_id");
            this.Property(t => t.local_interval_id).HasColumnName("local_interval_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.bridge_time_zone)
                .HasForeignKey(d => d.date_id);
            this.HasOptional(t => t.dim_date1)
                .WithMany(t => t.bridge_time_zone1)
                .HasForeignKey(d => d.local_date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.bridge_time_zone)
                .HasForeignKey(d => d.interval_id);
            this.HasOptional(t => t.dim_interval1)
                .WithMany(t => t.bridge_time_zone1)
                .HasForeignKey(d => d.local_interval_id);
            this.HasRequired(t => t.dim_time_zone)
                .WithMany(t => t.bridge_time_zone)
                .HasForeignKey(d => d.time_zone_id);

        }
    }
}
