using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_agentMap : EntityTypeConfiguration<fact_agent>
    {
        public fact_agentMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.acd_login_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.acd_login_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_agent", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.acd_login_id).HasColumnName("acd_login_id");
            this.Property(t => t.local_date_id).HasColumnName("local_date_id");
            this.Property(t => t.local_interval_id).HasColumnName("local_interval_id");
            this.Property(t => t.ready_time_s).HasColumnName("ready_time_s");
            this.Property(t => t.logged_in_time_s).HasColumnName("logged_in_time_s");
            this.Property(t => t.not_ready_time_s).HasColumnName("not_ready_time_s");
            this.Property(t => t.idle_time_s).HasColumnName("idle_time_s");
            this.Property(t => t.direct_outbound_calls).HasColumnName("direct_outbound_calls");
            this.Property(t => t.direct_outbound_talk_time_s).HasColumnName("direct_outbound_talk_time_s");
            this.Property(t => t.direct_incoming_calls).HasColumnName("direct_incoming_calls");
            this.Property(t => t.direct_incoming_calls_talk_time_s).HasColumnName("direct_incoming_calls_talk_time_s");
            this.Property(t => t.admin_time_s).HasColumnName("admin_time_s");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_acd_login)
                .WithMany(t => t.fact_agent)
                .HasForeignKey(d => d.acd_login_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_agent)
                .HasForeignKey(d => d.local_date_id);
            this.HasRequired(t => t.dim_date1)
                .WithMany(t => t.fact_agent1)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_agent)
                .HasForeignKey(d => d.local_interval_id);
            this.HasRequired(t => t.dim_interval1)
                .WithMany(t => t.fact_agent1)
                .HasForeignKey(d => d.interval_id);

        }
    }
}
