using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class permission_reportMap : EntityTypeConfiguration<permission_report>
    {
        public permission_reportMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_code, t.team_id, t.my_own, t.business_unit_id, t.ReportId });

            // Properties
            this.Property(t => t.team_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.business_unit_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("permission_report", "mart");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.my_own).HasColumnName("my_own");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.ReportId).HasColumnName("ReportId");
        }
    }
}
