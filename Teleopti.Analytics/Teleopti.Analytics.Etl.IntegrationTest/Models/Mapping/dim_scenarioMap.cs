using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_scenarioMap : EntityTypeConfiguration<dim_scenario>
    {
        public dim_scenarioMap()
        {
            // Primary Key
            this.HasKey(t => t.scenario_id);

            // Properties
            this.Property(t => t.scenario_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("dim_scenario", "mart");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.scenario_name).HasColumnName("scenario_name");
            this.Property(t => t.default_scenario).HasColumnName("default_scenario");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
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
