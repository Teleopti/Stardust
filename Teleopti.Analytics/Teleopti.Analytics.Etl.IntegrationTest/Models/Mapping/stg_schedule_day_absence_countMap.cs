using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_day_absence_countMap : EntityTypeConfiguration<stg_schedule_day_absence_count>
    {
        public stg_schedule_day_absence_countMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date, t.start_interval_id, t.person_code, t.scenario_code });

            // Properties
            this.Property(t => t.start_interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("stg_schedule_day_absence_count", "stage");
            this.Property(t => t.date).HasColumnName("date");
            this.Property(t => t.start_interval_id).HasColumnName("start_interval_id");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.starttime).HasColumnName("starttime");
            this.Property(t => t.absence_code).HasColumnName("absence_code");
            this.Property(t => t.day_count).HasColumnName("day_count");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
