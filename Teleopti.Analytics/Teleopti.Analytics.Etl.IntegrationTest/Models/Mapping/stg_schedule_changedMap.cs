using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_changedMap : EntityTypeConfiguration<stg_schedule_changed>
    {
        public stg_schedule_changedMap()
        {
            // Primary Key
            this.HasKey(t => new { t.schedule_date_local, t.person_code, t.scenario_code });

            // Properties
            // Table & Column Mappings
            this.ToTable("stg_schedule_changed", "stage");
			this.Property(t => t.schedule_date_local).HasColumnName("schedule_date_local");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
