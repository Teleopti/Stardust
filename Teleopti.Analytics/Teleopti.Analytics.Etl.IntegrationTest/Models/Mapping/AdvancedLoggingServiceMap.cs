using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class AdvancedLoggingServiceMap : EntityTypeConfiguration<AdvancedLoggingService>
    {
        public AdvancedLoggingServiceMap()
        {
            // Primary Key
            this.HasKey(t => t.LogDate);

            // Properties
            this.Property(t => t.Message)
                .HasMaxLength(100);

            this.Property(t => t.BU)
                .HasMaxLength(50);

            this.Property(t => t.DataSource)
                .HasMaxLength(200);

            this.Property(t => t.WindowsIdentity)
                .HasMaxLength(200);

            this.Property(t => t.HostIP)
                .HasMaxLength(30);

            this.Property(t => t.BlockOptions)
                .HasMaxLength(500);

            this.Property(t => t.TeamOptions)
                .HasMaxLength(500);

            this.Property(t => t.GeneralOptions)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("AdvancedLoggingService");
            this.Property(t => t.LogDate).HasColumnName("LogDate");
            this.Property(t => t.Message).HasColumnName("Message");
            this.Property(t => t.BU).HasColumnName("BU");
            this.Property(t => t.BUId).HasColumnName("BUId");
            this.Property(t => t.DataSource).HasColumnName("DataSource");
            this.Property(t => t.WindowsIdentity).HasColumnName("WindowsIdentity");
            this.Property(t => t.HostIP).HasColumnName("HostIP");
            this.Property(t => t.BlockOptions).HasColumnName("BlockOptions");
            this.Property(t => t.TeamOptions).HasColumnName("TeamOptions");
            this.Property(t => t.GeneralOptions).HasColumnName("GeneralOptions");
            this.Property(t => t.SkillDays).HasColumnName("SkillDays");
            this.Property(t => t.Agents).HasColumnName("Agents");
            this.Property(t => t.ExecutionTime).HasColumnName("ExecutionTime");
        }
    }
}
