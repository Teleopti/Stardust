using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_teamMap : EntityTypeConfiguration<dim_team>
    {
        public dim_teamMap()
        {
            // Primary Key
            this.HasKey(t => t.team_id);

            // Properties
            this.Property(t => t.team_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_team", "mart");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.team_code).HasColumnName("team_code");
            this.Property(t => t.team_name).HasColumnName("team_name");
            this.Property(t => t.scorecard_id).HasColumnName("scorecard_id");
            this.Property(t => t.site_id).HasColumnName("site_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasOptional(t => t.dim_scorecard)
                .WithMany(t => t.dim_team)
                .HasForeignKey(d => d.scorecard_id);
            this.HasOptional(t => t.dim_site)
                .WithMany(t => t.dim_team)
                .HasForeignKey(d => d.site_id);

        }
    }
}
