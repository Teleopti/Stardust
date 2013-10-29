using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_scorecardMap : EntityTypeConfiguration<stg_scorecard>
    {
        public stg_scorecardMap()
        {
            // Primary Key
            this.HasKey(t => t.scorecard_code);

            // Properties
            this.Property(t => t.scorecard_name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.period_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_scorecard", "stage");
            this.Property(t => t.scorecard_code).HasColumnName("scorecard_code");
            this.Property(t => t.scorecard_name).HasColumnName("scorecard_name");
            this.Property(t => t.period).HasColumnName("period");
            this.Property(t => t.period_name).HasColumnName("period_name");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
