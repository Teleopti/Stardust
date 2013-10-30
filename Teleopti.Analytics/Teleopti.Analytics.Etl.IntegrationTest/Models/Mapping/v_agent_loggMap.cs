using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_agent_loggMap : EntityTypeConfiguration<v_agent_logg>
    {
        public v_agent_loggMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue, t.date_from, t.interval, t.agent_id });

            // Properties
            this.Property(t => t.queue)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.agent_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.agent_name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("v_agent_logg", "mart");
            this.Property(t => t.queue).HasColumnName("queue");
            this.Property(t => t.date_from).HasColumnName("date_from");
            this.Property(t => t.interval).HasColumnName("interval");
            this.Property(t => t.agent_id).HasColumnName("agent_id");
            this.Property(t => t.agent_name).HasColumnName("agent_name");
            this.Property(t => t.avail_dur).HasColumnName("avail_dur");
            this.Property(t => t.tot_work_dur).HasColumnName("tot_work_dur");
            this.Property(t => t.talking_call_dur).HasColumnName("talking_call_dur");
            this.Property(t => t.pause_dur).HasColumnName("pause_dur");
            this.Property(t => t.wait_dur).HasColumnName("wait_dur");
            this.Property(t => t.wrap_up_dur).HasColumnName("wrap_up_dur");
            this.Property(t => t.answ_call_cnt).HasColumnName("answ_call_cnt");
            this.Property(t => t.direct_out_call_cnt).HasColumnName("direct_out_call_cnt");
            this.Property(t => t.direct_out_call_dur).HasColumnName("direct_out_call_dur");
            this.Property(t => t.direct_in_call_cnt).HasColumnName("direct_in_call_cnt");
            this.Property(t => t.direct_in_call_dur).HasColumnName("direct_in_call_dur");
            this.Property(t => t.transfer_out_call_cnt).HasColumnName("transfer_out_call_cnt");
            this.Property(t => t.admin_dur).HasColumnName("admin_dur");
        }
    }
}
