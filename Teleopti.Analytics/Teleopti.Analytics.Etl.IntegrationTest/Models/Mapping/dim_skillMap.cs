using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_skillMap : EntityTypeConfiguration<dim_skill>
    {
        public dim_skillMap()
        {
            // Primary Key
            this.HasKey(t => t.skill_id);

            // Properties
            this.Property(t => t.skill_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.forecast_method_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_skill", "mart");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.skill_code).HasColumnName("skill_code");
            this.Property(t => t.skill_name).HasColumnName("skill_name");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.forecast_method_code).HasColumnName("forecast_method_code");
            this.Property(t => t.forecast_method_name).HasColumnName("forecast_method_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
        }
    }
}
