using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_scheduleMap : EntityTypeConfiguration<stg_schedule>
    {
        public stg_scheduleMap()
        {
            // Primary Key
			this.HasKey(t => new { t.schedule_date_local, t.schedule_date_utc, t.person_code, t.interval_id, t.activity_start, t.scenario_code, t.shift_start });

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_schedule", "stage");
			this.Property(t => t.schedule_date_local).HasColumnName("schedule_date_local");
			this.Property(t => t.schedule_date_utc).HasColumnName("schedule_date_utc");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.activity_start).HasColumnName("activity_start");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.activity_code).HasColumnName("activity_code");
            this.Property(t => t.absence_code).HasColumnName("absence_code");
            this.Property(t => t.activity_end).HasColumnName("activity_end");
            this.Property(t => t.shift_start).HasColumnName("shift_start");
            this.Property(t => t.shift_end).HasColumnName("shift_end");
            this.Property(t => t.shift_startinterval_id).HasColumnName("shift_startinterval_id");
            this.Property(t => t.shift_category_code).HasColumnName("shift_category_code");
            this.Property(t => t.shift_length_m).HasColumnName("shift_length_m");
            this.Property(t => t.scheduled_time_m).HasColumnName("scheduled_time_m");
            this.Property(t => t.scheduled_time_absence_m).HasColumnName("scheduled_time_absence_m");
            this.Property(t => t.scheduled_time_activity_m).HasColumnName("scheduled_time_activity_m");
            this.Property(t => t.scheduled_contract_time_m).HasColumnName("scheduled_contract_time_m");
            this.Property(t => t.scheduled_contract_time_activity_m).HasColumnName("scheduled_contract_time_activity_m");
            this.Property(t => t.scheduled_contract_time_absence_m).HasColumnName("scheduled_contract_time_absence_m");
            this.Property(t => t.scheduled_work_time_m).HasColumnName("scheduled_work_time_m");
            this.Property(t => t.scheduled_work_time_activity_m).HasColumnName("scheduled_work_time_activity_m");
            this.Property(t => t.scheduled_work_time_absence_m).HasColumnName("scheduled_work_time_absence_m");
            this.Property(t => t.scheduled_over_time_m).HasColumnName("scheduled_over_time_m");
            this.Property(t => t.scheduled_ready_time_m).HasColumnName("scheduled_ready_time_m");
            this.Property(t => t.scheduled_paid_time_m).HasColumnName("scheduled_paid_time_m");
            this.Property(t => t.scheduled_paid_time_activity_m).HasColumnName("scheduled_paid_time_activity_m");
            this.Property(t => t.scheduled_paid_time_absence_m).HasColumnName("scheduled_paid_time_absence_m");
            this.Property(t => t.last_publish).HasColumnName("last_publish");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.overtime_code).HasColumnName("overtime_code");
        }
    }
}
