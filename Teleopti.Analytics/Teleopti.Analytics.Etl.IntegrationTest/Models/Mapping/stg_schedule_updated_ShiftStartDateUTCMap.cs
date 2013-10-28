using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_updated_ShiftStartDateUTCMap : EntityTypeConfiguration<stg_schedule_updated_ShiftStartDateUTC>
    {
        public stg_schedule_updated_ShiftStartDateUTCMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_id, t.shift_startdate_id });

            // Properties
            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.shift_startdate_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("stg_schedule_updated_ShiftStartDateUTC", "stage");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.shift_startdate_id).HasColumnName("shift_startdate_id");
        }
    }
}
