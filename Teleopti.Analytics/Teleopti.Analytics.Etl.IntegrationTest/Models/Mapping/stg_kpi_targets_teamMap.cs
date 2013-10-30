using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_kpi_targets_teamMap : EntityTypeConfiguration<stg_kpi_targets_team>
    {
        public stg_kpi_targets_teamMap()
        {
            // Primary Key
            this.HasKey(t => new { t.kpi_code, t.team_code });

            // Properties
            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_kpi_targets_team", "stage");
            this.Property(t => t.kpi_code).HasColumnName("kpi_code");
            this.Property(t => t.team_code).HasColumnName("team_code");
            this.Property(t => t.target_value).HasColumnName("target_value");
            this.Property(t => t.min_value).HasColumnName("min_value");
            this.Property(t => t.max_value).HasColumnName("max_value");
            this.Property(t => t.between_color).HasColumnName("between_color");
            this.Property(t => t.lower_than_min_color).HasColumnName("lower_than_min_color");
            this.Property(t => t.higher_than_max_color).HasColumnName("higher_than_max_color");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
