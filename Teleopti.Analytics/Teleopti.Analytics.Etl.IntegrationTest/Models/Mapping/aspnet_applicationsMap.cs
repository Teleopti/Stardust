using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class aspnet_applicationsMap : EntityTypeConfiguration<aspnet_applications>
    {
        public aspnet_applicationsMap()
        {
            // Primary Key
            this.HasKey(t => t.ApplicationId);

            // Properties
            this.Property(t => t.ApplicationName)
                .IsRequired()
                .HasMaxLength(256);

            this.Property(t => t.LoweredApplicationName)
                .IsRequired()
                .HasMaxLength(256);

            this.Property(t => t.Description)
                .HasMaxLength(256);

            // Table & Column Mappings
            this.ToTable("aspnet_applications");
            this.Property(t => t.ApplicationName).HasColumnName("ApplicationName");
            this.Property(t => t.LoweredApplicationName).HasColumnName("LoweredApplicationName");
            this.Property(t => t.ApplicationId).HasColumnName("ApplicationId");
            this.Property(t => t.Description).HasColumnName("Description");
        }
    }
}
