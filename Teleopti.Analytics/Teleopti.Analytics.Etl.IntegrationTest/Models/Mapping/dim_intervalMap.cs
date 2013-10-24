using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_intervalMap : EntityTypeConfiguration<dim_interval>
    {
        public dim_intervalMap()
        {
            // Primary Key
            this.HasKey(t => t.interval_id);

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_name)
                .HasMaxLength(20);

            this.Property(t => t.halfhour_name)
                .HasMaxLength(50);

            this.Property(t => t.hour_name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("dim_interval", "mart");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.interval_name).HasColumnName("interval_name");
            this.Property(t => t.halfhour_name).HasColumnName("halfhour_name");
            this.Property(t => t.hour_name).HasColumnName("hour_name");
            this.Property(t => t.interval_start).HasColumnName("interval_start");
            this.Property(t => t.interval_end).HasColumnName("interval_end");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
