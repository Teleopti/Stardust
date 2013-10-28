using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_schedule_deviationMap : EntityTypeConfiguration<fact_schedule_deviation>
    {
        public fact_schedule_deviationMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.person_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_schedule_deviation", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.scheduled_ready_time_s).HasColumnName("scheduled_ready_time_s");
            this.Property(t => t.ready_time_s).HasColumnName("ready_time_s");
            this.Property(t => t.contract_time_s).HasColumnName("contract_time_s");
            this.Property(t => t.deviation_schedule_s).HasColumnName("deviation_schedule_s");
            this.Property(t => t.deviation_schedule_ready_s).HasColumnName("deviation_schedule_ready_s");
            this.Property(t => t.deviation_contract_s).HasColumnName("deviation_contract_s");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.is_logged_in).HasColumnName("is_logged_in");
            this.Property(t => t.shift_startdate_id).HasColumnName("shift_startdate_id");
            this.Property(t => t.shift_startinterval_id).HasColumnName("shift_startinterval_id");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_schedule_deviation)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_schedule_deviation)
                .HasForeignKey(d => d.interval_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_schedule_deviation)
                .HasForeignKey(d => d.person_id);

        }
    }
}
