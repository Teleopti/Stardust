using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_scheduleMap : EntityTypeConfiguration<fact_schedule>
    {
        public fact_scheduleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.schedule_date_id, t.person_id, t.interval_id, t.activity_starttime, t.scenario_id });

            // Properties
            this.Property(t => t.schedule_date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_schedule", "mart");
            this.Property(t => t.schedule_date_id).HasColumnName("schedule_date_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.activity_starttime).HasColumnName("activity_starttime");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.activity_id).HasColumnName("activity_id");
            this.Property(t => t.absence_id).HasColumnName("absence_id");
            this.Property(t => t.activity_startdate_id).HasColumnName("activity_startdate_id");
            this.Property(t => t.activity_enddate_id).HasColumnName("activity_enddate_id");
            this.Property(t => t.activity_endtime).HasColumnName("activity_endtime");
            this.Property(t => t.shift_startdate_id).HasColumnName("shift_startdate_id");
            this.Property(t => t.shift_starttime).HasColumnName("shift_starttime");
            this.Property(t => t.shift_enddate_id).HasColumnName("shift_enddate_id");
            this.Property(t => t.shift_endtime).HasColumnName("shift_endtime");
            this.Property(t => t.shift_startinterval_id).HasColumnName("shift_startinterval_id");
            this.Property(t => t.shift_category_id).HasColumnName("shift_category_id");
            this.Property(t => t.shift_length_id).HasColumnName("shift_length_id");
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
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.overtime_id).HasColumnName("overtime_id");

            // Relationships
            this.HasOptional(t => t.dim_absence)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.absence_id);
            this.HasOptional(t => t.dim_activity)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.activity_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.schedule_date_id);
            this.HasOptional(t => t.dim_date1)
                .WithMany(t => t.fact_schedule1)
                .HasForeignKey(d => d.activity_startdate_id);
            this.HasOptional(t => t.dim_date2)
                .WithMany(t => t.fact_schedule2)
                .HasForeignKey(d => d.activity_enddate_id);
            this.HasOptional(t => t.dim_date3)
                .WithMany(t => t.fact_schedule3)
                .HasForeignKey(d => d.shift_startdate_id);
            this.HasOptional(t => t.dim_date4)
                .WithMany(t => t.fact_schedule4)
                .HasForeignKey(d => d.shift_enddate_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.interval_id);
            this.HasOptional(t => t.dim_interval1)
                .WithMany(t => t.fact_schedule1)
                .HasForeignKey(d => d.shift_startinterval_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_schedule)
                .HasForeignKey(d => d.scenario_id);

        }
    }
}
