using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class scorecard_kpiMap : EntityTypeConfiguration<scorecard_kpi>
    {
        public scorecard_kpiMap()
        {
            // Primary Key
            this.HasKey(t => new { t.scorecard_id, t.kpi_id });

            // Properties
            this.Property(t => t.scorecard_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.kpi_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("scorecard_kpi", "mart");
            this.Property(t => t.scorecard_id).HasColumnName("scorecard_id");
            this.Property(t => t.kpi_id).HasColumnName("kpi_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_kpi)
                .WithMany(t => t.scorecard_kpi)
                .HasForeignKey(d => d.kpi_id);
            this.HasRequired(t => t.dim_scorecard)
                .WithMany(t => t.scorecard_kpi)
                .HasForeignKey(d => d.scorecard_id);

        }
    }
}
