using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_forecast_workloadMap : EntityTypeConfiguration<fact_forecast_workload>
    {
        public fact_forecast_workloadMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.start_time, t.workload_id, t.scenario_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.workload_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_forecast_workload", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.start_time).HasColumnName("start_time");
            this.Property(t => t.workload_id).HasColumnName("workload_id");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.end_time).HasColumnName("end_time");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.forecasted_calls).HasColumnName("forecasted_calls");
            this.Property(t => t.forecasted_emails).HasColumnName("forecasted_emails");
            this.Property(t => t.forecasted_backoffice_tasks).HasColumnName("forecasted_backoffice_tasks");
            this.Property(t => t.forecasted_campaign_calls).HasColumnName("forecasted_campaign_calls");
            this.Property(t => t.forecasted_calls_excl_campaign).HasColumnName("forecasted_calls_excl_campaign");
            this.Property(t => t.forecasted_talk_time_s).HasColumnName("forecasted_talk_time_s");
            this.Property(t => t.forecasted_campaign_talk_time_s).HasColumnName("forecasted_campaign_talk_time_s");
            this.Property(t => t.forecasted_talk_time_excl_campaign_s).HasColumnName("forecasted_talk_time_excl_campaign_s");
            this.Property(t => t.forecasted_after_call_work_s).HasColumnName("forecasted_after_call_work_s");
            this.Property(t => t.forecasted_campaign_after_call_work_s).HasColumnName("forecasted_campaign_after_call_work_s");
            this.Property(t => t.forecasted_after_call_work_excl_campaign_s).HasColumnName("forecasted_after_call_work_excl_campaign_s");
            this.Property(t => t.forecasted_handling_time_s).HasColumnName("forecasted_handling_time_s");
            this.Property(t => t.forecasted_campaign_handling_time_s).HasColumnName("forecasted_campaign_handling_time_s");
            this.Property(t => t.forecasted_handling_time_excl_campaign_s).HasColumnName("forecasted_handling_time_excl_campaign_s");
            this.Property(t => t.period_length_min).HasColumnName("period_length_min");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_forecast_workload)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_forecast_workload)
                .HasForeignKey(d => d.interval_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_forecast_workload)
                .HasForeignKey(d => d.scenario_id);
            this.HasOptional(t => t.dim_skill)
                .WithMany(t => t.fact_forecast_workload)
                .HasForeignKey(d => d.skill_id);
            this.HasRequired(t => t.dim_workload)
                .WithMany(t => t.fact_forecast_workload)
                .HasForeignKey(d => d.workload_id);

        }
    }
}
