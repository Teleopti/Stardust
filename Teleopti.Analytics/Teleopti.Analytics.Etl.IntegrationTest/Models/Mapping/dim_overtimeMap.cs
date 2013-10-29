using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_overtimeMap : EntityTypeConfiguration<dim_overtime>
    {
        public dim_overtimeMap()
        {
            // Primary Key
            this.HasKey(t => t.overtime_id);

            // Properties
            this.Property(t => t.overtime_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_overtime", "mart");
            this.Property(t => t.overtime_id).HasColumnName("overtime_id");
            this.Property(t => t.overtime_code).HasColumnName("overtime_code");
            this.Property(t => t.overtime_name).HasColumnName("overtime_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
        }
    }
}
