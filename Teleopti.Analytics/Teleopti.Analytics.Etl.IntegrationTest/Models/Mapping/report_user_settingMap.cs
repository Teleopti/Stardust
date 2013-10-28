using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class report_user_settingMap : EntityTypeConfiguration<report_user_setting>
    {
        public report_user_settingMap()
        {
            // Primary Key
            this.HasKey(t => new { t.ReportId, t.person_code, t.param_name, t.saved_name_id });

            // Properties
            this.Property(t => t.param_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.saved_name_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("report_user_setting", "mart");
            this.Property(t => t.ReportId).HasColumnName("ReportId");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.param_name).HasColumnName("param_name");
            this.Property(t => t.saved_name_id).HasColumnName("saved_name_id");
            this.Property(t => t.control_setting).HasColumnName("control_setting");
        }
    }
}
