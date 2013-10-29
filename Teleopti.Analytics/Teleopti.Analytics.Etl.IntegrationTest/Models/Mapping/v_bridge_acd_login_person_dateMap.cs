using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_bridge_acd_login_person_dateMap : EntityTypeConfiguration<v_bridge_acd_login_person_date>
    {
        public v_bridge_acd_login_person_dateMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.acd_login_id, t.person_id, t.valid_from_date_id, t.valid_to_date_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.acd_login_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.valid_from_date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.valid_to_date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("v_bridge_acd_login_person_date", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.acd_login_id).HasColumnName("acd_login_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.valid_from_date_id).HasColumnName("valid_from_date_id");
            this.Property(t => t.valid_to_date_id).HasColumnName("valid_to_date_id");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
        }
    }
}
