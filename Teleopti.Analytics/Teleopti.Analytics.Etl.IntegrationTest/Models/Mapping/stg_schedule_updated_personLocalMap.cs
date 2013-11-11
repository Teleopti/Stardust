using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_schedule_updated_personLocalMap : EntityTypeConfiguration<stg_schedule_updated_personLocal>
    {
        public stg_schedule_updated_personLocalMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_id, t.valid_from_date_local, t.valid_to_date_local });

            // Properties
            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("stg_schedule_updated_personLocal", "stage");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.valid_from_date_local).HasColumnName("valid_from_date_local");
            this.Property(t => t.valid_to_date_local).HasColumnName("valid_to_date_local");
        }
    }
}
