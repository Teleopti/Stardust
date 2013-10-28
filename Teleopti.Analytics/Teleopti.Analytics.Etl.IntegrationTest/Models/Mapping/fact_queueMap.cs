using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_queueMap : EntityTypeConfiguration<fact_queue>
    {
        public fact_queueMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.queue_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.queue_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_queue", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.queue_id).HasColumnName("queue_id");
            this.Property(t => t.local_date_id).HasColumnName("local_date_id");
            this.Property(t => t.local_interval_id).HasColumnName("local_interval_id");
            this.Property(t => t.offered_calls).HasColumnName("offered_calls");
            this.Property(t => t.answered_calls).HasColumnName("answered_calls");
            this.Property(t => t.answered_calls_within_SL).HasColumnName("answered_calls_within_SL");
            this.Property(t => t.abandoned_calls).HasColumnName("abandoned_calls");
            this.Property(t => t.abandoned_calls_within_SL).HasColumnName("abandoned_calls_within_SL");
            this.Property(t => t.abandoned_short_calls).HasColumnName("abandoned_short_calls");
            this.Property(t => t.overflow_out_calls).HasColumnName("overflow_out_calls");
            this.Property(t => t.overflow_in_calls).HasColumnName("overflow_in_calls");
            this.Property(t => t.talk_time_s).HasColumnName("talk_time_s");
            this.Property(t => t.after_call_work_s).HasColumnName("after_call_work_s");
            this.Property(t => t.handle_time_s).HasColumnName("handle_time_s");
            this.Property(t => t.speed_of_answer_s).HasColumnName("speed_of_answer_s");
            this.Property(t => t.time_to_abandon_s).HasColumnName("time_to_abandon_s");
            this.Property(t => t.longest_delay_in_queue_answered_s).HasColumnName("longest_delay_in_queue_answered_s");
            this.Property(t => t.longest_delay_in_queue_abandoned_s).HasColumnName("longest_delay_in_queue_abandoned_s");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_queue)
                .HasForeignKey(d => d.local_date_id);
            this.HasRequired(t => t.dim_date1)
                .WithMany(t => t.fact_queue1)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_queue)
                .HasForeignKey(d => d.local_interval_id);
            this.HasRequired(t => t.dim_interval1)
                .WithMany(t => t.fact_queue1)
                .HasForeignKey(d => d.interval_id);
            this.HasRequired(t => t.dim_queue)
                .WithMany(t => t.fact_queue)
                .HasForeignKey(d => d.queue_id);

        }
    }
}
