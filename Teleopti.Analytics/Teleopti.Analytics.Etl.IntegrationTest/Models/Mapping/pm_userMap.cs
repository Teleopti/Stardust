using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class pm_userMap : EntityTypeConfiguration<pm_user>
    {
        public pm_userMap()
        {
            // Primary Key
            this.HasKey(t => new { t.user_name, t.is_windows_logon });

            // Properties
            this.Property(t => t.user_name)
                .IsRequired()
                .HasMaxLength(256);

            // Table & Column Mappings
            this.ToTable("pm_user", "mart");
            this.Property(t => t.user_name).HasColumnName("user_name");
            this.Property(t => t.is_windows_logon).HasColumnName("is_windows_logon");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
