using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_queue_loggMap : EntityTypeConfiguration<v_queue_logg>
    {
        public v_queue_loggMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue, t.date_from, t.interval });

            // Properties
            this.Property(t => t.queue)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("v_queue_logg", "mart");
            this.Property(t => t.queue).HasColumnName("queue");
            this.Property(t => t.date_from).HasColumnName("date_from");
            this.Property(t => t.interval).HasColumnName("interval");
            this.Property(t => t.offd_direct_call_cnt).HasColumnName("offd_direct_call_cnt");
            this.Property(t => t.overflow_in_call_cnt).HasColumnName("overflow_in_call_cnt");
            this.Property(t => t.aband_call_cnt).HasColumnName("aband_call_cnt");
            this.Property(t => t.overflow_out_call_cnt).HasColumnName("overflow_out_call_cnt");
            this.Property(t => t.answ_call_cnt).HasColumnName("answ_call_cnt");
            this.Property(t => t.queued_and_answ_call_dur).HasColumnName("queued_and_answ_call_dur");
            this.Property(t => t.queued_and_aband_call_dur).HasColumnName("queued_and_aband_call_dur");
            this.Property(t => t.talking_call_dur).HasColumnName("talking_call_dur");
            this.Property(t => t.wrap_up_dur).HasColumnName("wrap_up_dur");
            this.Property(t => t.queued_answ_longest_que_dur).HasColumnName("queued_answ_longest_que_dur");
            this.Property(t => t.queued_aband_longest_que_dur).HasColumnName("queued_aband_longest_que_dur");
            this.Property(t => t.avg_avail_member_cnt).HasColumnName("avg_avail_member_cnt");
            this.Property(t => t.ans_servicelevel_cnt).HasColumnName("ans_servicelevel_cnt");
            this.Property(t => t.wait_dur).HasColumnName("wait_dur");
            this.Property(t => t.aband_short_call_cnt).HasColumnName("aband_short_call_cnt");
            this.Property(t => t.aband_within_sl_cnt).HasColumnName("aband_within_sl_cnt");
        }
    }
}
