using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_agent_infoMap : EntityTypeConfiguration<v_agent_info>
    {
        public v_agent_infoMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Agent_id, t.Agent_name, t.log_object_id, t.orig_agent_id });

            // Properties
            this.Property(t => t.Agent_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.Agent_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.log_object_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.orig_agent_id)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("v_agent_info", "mart");
            this.Property(t => t.Agent_id).HasColumnName("Agent_id");
            this.Property(t => t.Agent_name).HasColumnName("Agent_name");
            this.Property(t => t.is_active).HasColumnName("is_active");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.orig_agent_id).HasColumnName("orig_agent_id");
        }
    }
}
