using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class bridge_group_page_personMap : EntityTypeConfiguration<bridge_group_page_person>
    {
        public bridge_group_page_personMap()
        {
            // Primary Key
            this.HasKey(t => new { t.group_page_id, t.person_id });

            // Properties
            this.Property(t => t.group_page_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("bridge_group_page_person", "mart");
            this.Property(t => t.group_page_id).HasColumnName("group_page_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");

            // Relationships
            this.HasRequired(t => t.dim_group_page)
                .WithMany(t => t.bridge_group_page_person)
                .HasForeignKey(d => d.group_page_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.bridge_group_page_person)
                .HasForeignKey(d => d.person_id);

        }
    }
}
