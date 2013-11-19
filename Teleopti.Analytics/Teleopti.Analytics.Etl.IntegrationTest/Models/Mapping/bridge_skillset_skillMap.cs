using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class bridge_skillset_skillMap : EntityTypeConfiguration<bridge_skillset_skill>
    {
        public bridge_skillset_skillMap()
        {
            // Primary Key
            this.HasKey(t => new { t.skillset_id, t.skill_id });

            // Properties
            this.Property(t => t.skillset_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.skill_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("bridge_skillset_skill", "mart");
            this.Property(t => t.skillset_id).HasColumnName("skillset_id");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_skill)
                .WithMany(t => t.bridge_skillset_skill)
                .HasForeignKey(d => d.skill_id);
            this.HasRequired(t => t.dim_skillset)
                .WithMany(t => t.bridge_skillset_skill)
                .HasForeignKey(d => d.skillset_id);

        }
    }
}
