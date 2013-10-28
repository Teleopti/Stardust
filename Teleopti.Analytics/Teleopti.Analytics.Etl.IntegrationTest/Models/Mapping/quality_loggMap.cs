using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class quality_loggMap : EntityTypeConfiguration<quality_logg>
    {
        public quality_loggMap()
        {
            // Primary Key
            this.HasKey(t => new { t.quality_id, t.date_from, t.agent_id, t.evaluation_id });

            // Properties
            this.Property(t => t.quality_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.agent_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.evaluation_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("quality_logg");
            this.Property(t => t.quality_id).HasColumnName("quality_id");
            this.Property(t => t.date_from).HasColumnName("date_from");
            this.Property(t => t.agent_id).HasColumnName("agent_id");
            this.Property(t => t.evaluation_id).HasColumnName("evaluation_id");
            this.Property(t => t.score).HasColumnName("score");

            // Relationships
            this.HasRequired(t => t.agent_info)
                .WithMany(t => t.quality_logg)
                .HasForeignKey(d => d.agent_id);
            this.HasRequired(t => t.quality_info)
                .WithMany(t => t.quality_logg)
                .HasForeignKey(d => d.quality_id);

        }
    }
}
