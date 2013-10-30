using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_day_offMap : EntityTypeConfiguration<stg_day_off>
    {
        public stg_day_offMap()
        {
            // Primary Key
            this.HasKey(t => new { t.day_off_name, t.business_unit_code });

            // Properties
            this.Property(t => t.day_off_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.day_off_shortname)
                .IsRequired()
                .HasMaxLength(25);

            this.Property(t => t.display_color_html)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(7);

            // Table & Column Mappings
            this.ToTable("stg_day_off", "stage");
            this.Property(t => t.day_off_code).HasColumnName("day_off_code");
            this.Property(t => t.day_off_name).HasColumnName("day_off_name");
            this.Property(t => t.day_off_shortname).HasColumnName("day_off_shortname");
            this.Property(t => t.display_color).HasColumnName("display_color");
            this.Property(t => t.display_color_html).HasColumnName("display_color_html");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
