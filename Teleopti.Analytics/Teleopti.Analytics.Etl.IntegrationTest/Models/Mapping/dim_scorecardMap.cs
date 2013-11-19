using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_scorecardMap : EntityTypeConfiguration<dim_scorecard>
    {
        public dim_scorecardMap()
        {
            // Primary Key
            this.HasKey(t => t.scorecard_id);

            // Properties
            this.Property(t => t.scorecard_name)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("dim_scorecard", "mart");
            this.Property(t => t.scorecard_id).HasColumnName("scorecard_id");
            this.Property(t => t.scorecard_code).HasColumnName("scorecard_code");
            this.Property(t => t.scorecard_name).HasColumnName("scorecard_name");
            this.Property(t => t.period).HasColumnName("period");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
