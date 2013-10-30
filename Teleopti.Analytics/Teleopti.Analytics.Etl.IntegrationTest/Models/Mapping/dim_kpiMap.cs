using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_kpiMap : EntityTypeConfiguration<dim_kpi>
    {
        public dim_kpiMap()
        {
            // Primary Key
            this.HasKey(t => t.kpi_id);

            // Properties
            this.Property(t => t.kpi_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.resource_key)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("dim_kpi", "mart");
            this.Property(t => t.kpi_id).HasColumnName("kpi_id");
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
            this.Property(t => t.decreasing_value_is_positive).HasColumnName("decreasing_value_is_positive");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
