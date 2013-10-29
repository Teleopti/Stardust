using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_preferenceMap : EntityTypeConfiguration<stg_schedule_preference>
    {
        public stg_schedule_preferenceMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_restriction_code, t.scenario_code });

            // Properties
            this.Property(t => t.day_off_name)
                .HasMaxLength(50);

            this.Property(t => t.day_off_shortname)
                .HasMaxLength(25);

            // Table & Column Mappings
            this.ToTable("stg_schedule_preference", "stage");
            this.Property(t => t.person_restriction_code).HasColumnName("person_restriction_code");
            this.Property(t => t.restriction_date).HasColumnName("restriction_date");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.scenario_code).HasColumnName("scenario_code");
            this.Property(t => t.shift_category_code).HasColumnName("shift_category_code");
            this.Property(t => t.day_off_code).HasColumnName("day_off_code");
            this.Property(t => t.day_off_name).HasColumnName("day_off_name");
            this.Property(t => t.day_off_shortname).HasColumnName("day_off_shortname");
            this.Property(t => t.StartTimeMinimum).HasColumnName("StartTimeMinimum");
            this.Property(t => t.StartTimeMaximum).HasColumnName("StartTimeMaximum");
            this.Property(t => t.endTimeMinimum).HasColumnName("endTimeMinimum");
            this.Property(t => t.endTimeMaximum).HasColumnName("endTimeMaximum");
            this.Property(t => t.WorkTimeMinimum).HasColumnName("WorkTimeMinimum");
            this.Property(t => t.WorkTimeMaximum).HasColumnName("WorkTimeMaximum");
            this.Property(t => t.preference_fulfilled).HasColumnName("preference_fulfilled");
            this.Property(t => t.preference_unfulfilled).HasColumnName("preference_unfulfilled");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.activity_code).HasColumnName("activity_code");
            this.Property(t => t.absence_code).HasColumnName("absence_code");
            this.Property(t => t.must_have).HasColumnName("must_have");
        }
    }
}
