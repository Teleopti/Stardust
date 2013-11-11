using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_qualityMap : EntityTypeConfiguration<fact_quality>
    {
        public fact_qualityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.evaluation_id, t.datasource_id });

            // Properties
            this.Property(t => t.evaluation_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.datasource_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_quality", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.acd_login_id).HasColumnName("acd_login_id");
            this.Property(t => t.evaluation_id).HasColumnName("evaluation_id");
            this.Property(t => t.quality_quest_id).HasColumnName("quality_quest_id");
            this.Property(t => t.score).HasColumnName("score");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");

            // Relationships
            this.HasRequired(t => t.dim_acd_login)
                .WithMany(t => t.fact_quality)
                .HasForeignKey(d => d.acd_login_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_quality)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_quality_quest)
                .WithMany(t => t.fact_quality)
                .HasForeignKey(d => d.quality_quest_id);

        }
    }
}
