using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_schedule_forecast_skillMap : EntityTypeConfiguration<fact_schedule_forecast_skill>
    {
        public fact_schedule_forecast_skillMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.skill_id, t.scenario_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.skill_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_schedule_forecast_skill", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.forecasted_resources_m).HasColumnName("forecasted_resources_m");
            this.Property(t => t.forecasted_resources).HasColumnName("forecasted_resources");
            this.Property(t => t.forecasted_resources_incl_shrinkage_m).HasColumnName("forecasted_resources_incl_shrinkage_m");
            this.Property(t => t.forecasted_resources_incl_shrinkage).HasColumnName("forecasted_resources_incl_shrinkage");
            this.Property(t => t.scheduled_resources_m).HasColumnName("scheduled_resources_m");
            this.Property(t => t.scheduled_resources).HasColumnName("scheduled_resources");
            this.Property(t => t.scheduled_resources_incl_shrinkage_m).HasColumnName("scheduled_resources_incl_shrinkage_m");
            this.Property(t => t.scheduled_resources_incl_shrinkage).HasColumnName("scheduled_resources_incl_shrinkage");
            this.Property(t => t.intraday_deviation_m).HasColumnName("intraday_deviation_m");
            this.Property(t => t.relative_difference).HasColumnName("relative_difference");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_schedule_forecast_skill)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_schedule_forecast_skill)
                .HasForeignKey(d => d.interval_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_schedule_forecast_skill)
                .HasForeignKey(d => d.scenario_id);
            this.HasRequired(t => t.dim_skill)
                .WithMany(t => t.fact_schedule_forecast_skill)
                .HasForeignKey(d => d.skill_id);

        }
    }
}
