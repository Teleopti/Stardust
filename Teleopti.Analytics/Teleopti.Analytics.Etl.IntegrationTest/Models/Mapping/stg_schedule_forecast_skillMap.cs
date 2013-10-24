using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_forecast_skillMap : EntityTypeConfiguration<stg_schedule_forecast_skill>
    {
        public stg_schedule_forecast_skillMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date, t.interval_id, t.skill_code, t.scenario_code });

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_schedule_forecast_skill", "stage");
            this.Property(t => t.date).HasColumnName("date");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.skill_code).HasColumnName("skill_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.forecasted_resources_m).HasColumnName("forecasted_resources_m");
            this.Property(t => t.forecasted_resources).HasColumnName("forecasted_resources");
            this.Property(t => t.forecasted_resources_incl_shrinkage_m).HasColumnName("forecasted_resources_incl_shrinkage_m");
            this.Property(t => t.forecasted_resources_incl_shrinkage).HasColumnName("forecasted_resources_incl_shrinkage");
            this.Property(t => t.scheduled_resources_m).HasColumnName("scheduled_resources_m");
            this.Property(t => t.scheduled_resources).HasColumnName("scheduled_resources");
            this.Property(t => t.scheduled_resources_incl_shrinkage_m).HasColumnName("scheduled_resources_incl_shrinkage_m");
            this.Property(t => t.scheduled_resources_incl_shrinkage).HasColumnName("scheduled_resources_incl_shrinkage");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
