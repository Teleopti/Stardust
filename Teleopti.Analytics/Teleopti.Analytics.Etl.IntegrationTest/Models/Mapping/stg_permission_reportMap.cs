using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_permission_reportMap : EntityTypeConfiguration<stg_permission_report>
    {
        public stg_permission_reportMap()
        {
            // Primary Key
            this.HasKey(t => new { t.business_unit_code, t.business_unit_name });

            // Properties
            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_permission_report", "stage");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.ReportId).HasColumnName("ReportId");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.my_own).HasColumnName("my_own");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
