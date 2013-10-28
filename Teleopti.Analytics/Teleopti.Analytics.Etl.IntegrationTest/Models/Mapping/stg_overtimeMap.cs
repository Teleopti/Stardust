using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_overtimeMap : EntityTypeConfiguration<stg_overtime>
    {
        public stg_overtimeMap()
        {
            // Primary Key
            this.HasKey(t => new { t.overtime_name, t.business_unit_code, t.business_unit_name, t.datasource_id, t.insert_date, t.update_date, t.is_deleted });

            // Properties
            this.Property(t => t.overtime_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.datasource_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("stg_overtime", "stage");
            this.Property(t => t.overtime_code).HasColumnName("overtime_code");
            this.Property(t => t.overtime_name).HasColumnName("overtime_name");
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
