using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_quality_questMap : EntityTypeConfiguration<dim_quality_quest>
    {
        public dim_quality_questMap()
        {
            // Primary Key
            this.HasKey(t => t.quality_quest_id);

            // Properties
            this.Property(t => t.quality_quest_name)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.quality_quest_type_name)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.log_object_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_quality_quest", "mart");
            this.Property(t => t.quality_quest_id).HasColumnName("quality_quest_id");
            this.Property(t => t.quality_quest_agg_id).HasColumnName("quality_quest_agg_id");
            this.Property(t => t.quality_quest_original_id).HasColumnName("quality_quest_original_id");
            this.Property(t => t.quality_quest_score_weight).HasColumnName("quality_quest_score_weight");
            this.Property(t => t.quality_quest_name).HasColumnName("quality_quest_name");
            this.Property(t => t.quality_quest_type_name).HasColumnName("quality_quest_type_name");
            this.Property(t => t.log_object_name).HasColumnName("log_object_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
