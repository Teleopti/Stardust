using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_kpi_targets_teamMap : EntityTypeConfiguration<fact_kpi_targets_team>
    {
        public fact_kpi_targets_teamMap()
        {
            // Primary Key
            this.HasKey(t => new { t.kpi_id, t.team_id });

            // Properties
            this.Property(t => t.kpi_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.team_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_kpi_targets_team", "mart");
            this.Property(t => t.kpi_id).HasColumnName("kpi_id");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.target_value).HasColumnName("target_value");
            this.Property(t => t.min_value).HasColumnName("min_value");
            this.Property(t => t.max_value).HasColumnName("max_value");
            this.Property(t => t.between_color).HasColumnName("between_color");
            this.Property(t => t.lower_than_min_color).HasColumnName("lower_than_min_color");
            this.Property(t => t.higher_than_max_color).HasColumnName("higher_than_max_color");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_business_unit)
                .WithMany(t => t.fact_kpi_targets_team)
                .HasForeignKey(d => d.business_unit_id);
            this.HasRequired(t => t.dim_kpi)
                .WithMany(t => t.fact_kpi_targets_team)
                .HasForeignKey(d => d.kpi_id);
            this.HasRequired(t => t.dim_team)
                .WithMany(t => t.fact_kpi_targets_team)
                .HasForeignKey(d => d.team_id);

        }
    }
}
