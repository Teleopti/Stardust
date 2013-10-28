using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_forecast_workloadMap : EntityTypeConfiguration<stg_forecast_workload>
    {
        public stg_forecast_workloadMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date, t.interval_id, t.start_time, t.workload_code, t.scenario_code });

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.business_unit_name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_forecast_workload", "stage");
            this.Property(t => t.date).HasColumnName("date");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.start_time).HasColumnName("start_time");
            this.Property(t => t.workload_code).HasColumnName("workload_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.end_time).HasColumnName("end_time");
            this.Property(t => t.skill_code).HasColumnName("skill_code");
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
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
