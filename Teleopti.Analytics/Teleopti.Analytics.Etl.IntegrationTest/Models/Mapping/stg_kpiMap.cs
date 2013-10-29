using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_kpiMap : EntityTypeConfiguration<stg_kpi>
    {
        public stg_kpiMap()
        {
            // Primary Key
            this.HasKey(t => t.kpi_code);

            // Properties
            this.Property(t => t.kpi_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.resource_key)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_kpi", "stage");
            this.Property(t => t.kpi_code).HasColumnName("kpi_code");
            this.Property(t => t.kpi_name).HasColumnName("kpi_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
            this.Property(t => t.target_value_type).HasColumnName("target_value_type");
            this.Property(t => t.default_target_value).HasColumnName("default_target_value");
            this.Property(t => t.default_min_value).HasColumnName("default_min_value");
            this.Property(t => t.default_max_value).HasColumnName("default_max_value");
            this.Property(t => t.default_between_color).HasColumnName("default_between_color");
            this.Property(t => t.default_lower_than_min_color).HasColumnName("default_lower_than_min_color");
            this.Property(t => t.default_higher_than_max_color).HasColumnName("default_higher_than_max_color");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
