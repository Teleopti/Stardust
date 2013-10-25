using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class bridge_acd_login_personMap : EntityTypeConfiguration<bridge_acd_login_person>
    {
        public bridge_acd_login_personMap()
        {
            // Primary Key
            this.HasKey(t => new { t.acd_login_id, t.person_id });

            // Properties
            this.Property(t => t.acd_login_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("bridge_acd_login_person", "mart");
            this.Property(t => t.acd_login_id).HasColumnName("acd_login_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_acd_login)
                .WithMany(t => t.bridge_acd_login_person)
                .HasForeignKey(d => d.acd_login_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.bridge_acd_login_person)
                .HasForeignKey(d => d.person_id);
            this.HasOptional(t => t.dim_team)
                .WithMany(t => t.bridge_acd_login_person)
                .HasForeignKey(d => d.team_id);

        }
    }
}
