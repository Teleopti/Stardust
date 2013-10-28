using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class quality_infoMap : EntityTypeConfiguration<quality_info>
    {
        public quality_infoMap()
        {
            // Primary Key
            this.HasKey(t => t.quality_id);

            // Properties
            this.Property(t => t.quality_name)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.quality_type)
                .IsRequired()
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("quality_info");
            this.Property(t => t.quality_id).HasColumnName("quality_id");
            this.Property(t => t.quality_name).HasColumnName("quality_name");
            this.Property(t => t.quality_type).HasColumnName("quality_type");
            this.Property(t => t.score_weight).HasColumnName("score_weight");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.original_id).HasColumnName("original_id");

            // Relationships
            this.HasRequired(t => t.log_object)
                .WithMany(t => t.quality_info)
                .HasForeignKey(d => d.log_object_id);

        }
    }
}
