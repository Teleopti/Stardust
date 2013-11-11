using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_shift_categoryMap : EntityTypeConfiguration<stg_shift_category>
    {
        public stg_shift_categoryMap()
        {
            // Primary Key
            this.HasKey(t => t.shift_category_code);

            // Properties
            this.Property(t => t.shift_category_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.shift_category_shortname)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("stg_shift_category", "stage");
            this.Property(t => t.shift_category_code).HasColumnName("shift_category_code");
            this.Property(t => t.shift_category_name).HasColumnName("shift_category_name");
            this.Property(t => t.shift_category_shortname).HasColumnName("shift_category_shortname");
            this.Property(t => t.display_color).HasColumnName("display_color");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
        }
    }
}
