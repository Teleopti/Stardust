using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_stg_queueMap : EntityTypeConfiguration<v_stg_queue>
    {
        public v_stg_queueMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date, t.interval, t.queue_name });

            // Properties
            this.Property(t => t.interval)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.queue_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("v_stg_queue", "mart");
            this.Property(t => t.date).HasColumnName("date");
            this.Property(t => t.interval).HasColumnName("interval");
            this.Property(t => t.queue_code).HasColumnName("queue_code");
            this.Property(t => t.queue_name).HasColumnName("queue_name");
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
