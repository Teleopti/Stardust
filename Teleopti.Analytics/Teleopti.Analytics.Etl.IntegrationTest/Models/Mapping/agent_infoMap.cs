using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class agent_infoMap : EntityTypeConfiguration<agent_info>
    {
        public agent_infoMap()
        {
            // Primary Key
            this.HasKey(t => t.Agent_id);

            // Properties
            this.Property(t => t.Agent_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.orig_agent_id)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("agent_info");
            this.Property(t => t.Agent_id).HasColumnName("Agent_id");
            this.Property(t => t.Agent_name).HasColumnName("Agent_name");
            this.Property(t => t.is_active).HasColumnName("is_active");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.orig_agent_id).HasColumnName("orig_agent_id");

            // Relationships
            this.HasRequired(t => t.log_object)
                .WithMany(t => t.agent_info)
                .HasForeignKey(d => d.log_object_id);

        }
    }
}
