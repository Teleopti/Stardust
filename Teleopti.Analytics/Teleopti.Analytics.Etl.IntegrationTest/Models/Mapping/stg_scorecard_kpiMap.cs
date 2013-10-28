using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_scorecard_kpiMap : EntityTypeConfiguration<stg_scorecard_kpi>
    {
        public stg_scorecard_kpiMap()
        {
            // Primary Key
            this.HasKey(t => new { t.scorecard_code, t.kpi_code });

            // Properties
            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_scorecard_kpi", "stage");
            this.Property(t => t.scorecard_code).HasColumnName("scorecard_code");
            this.Property(t => t.kpi_code).HasColumnName("kpi_code");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
