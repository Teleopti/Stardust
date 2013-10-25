using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class language_translationMap : EntityTypeConfiguration<language_translation>
    {
        public language_translationMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Culture, t.ResourceKey });

            // Properties
            this.Property(t => t.Culture)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.ResourceKey)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.term_language)
                .IsRequired()
                .HasMaxLength(1000);

            this.Property(t => t.term_english)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("language_translation", "mart");
            this.Property(t => t.Culture).HasColumnName("Culture");
            this.Property(t => t.language_id).HasColumnName("language_id");
            this.Property(t => t.ResourceKey).HasColumnName("ResourceKey");
            this.Property(t => t.term_language).HasColumnName("term_language");
            this.Property(t => t.term_english).HasColumnName("term_english");
        }
    }
}
