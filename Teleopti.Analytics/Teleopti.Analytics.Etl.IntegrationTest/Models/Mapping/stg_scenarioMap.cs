using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_scenarioMap : EntityTypeConfiguration<stg_scenario>
    {
        public stg_scenarioMap()
        {
            // Primary Key
            this.HasKey(t => t.scenario_code);

            // Properties
            this.Property(t => t.scenario_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_scenario", "stage");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.scenario_name).HasColumnName("scenario_name");
            this.Property(t => t.default_scenario).HasColumnName("default_scenario");
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
