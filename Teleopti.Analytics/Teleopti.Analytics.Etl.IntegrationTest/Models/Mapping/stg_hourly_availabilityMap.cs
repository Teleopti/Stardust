using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_hourly_availabilityMap : EntityTypeConfiguration<stg_hourly_availability>
    {
        public stg_hourly_availabilityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.restriction_date, t.person_code, t.scenario_code });

            // Properties
            // Table & Column Mappings
            this.ToTable("stg_hourly_availability", "stage");
            this.Property(t => t.restriction_date).HasColumnName("restriction_date");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.available_time_m).HasColumnName("available_time_m");
            this.Property(t => t.scheduled_time_m).HasColumnName("scheduled_time_m");
            this.Property(t => t.scheduled).HasColumnName("scheduled");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
        }
    }
}
