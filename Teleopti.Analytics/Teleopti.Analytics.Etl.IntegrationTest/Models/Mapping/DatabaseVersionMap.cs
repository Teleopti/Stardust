using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class DatabaseVersionMap : EntityTypeConfiguration<DatabaseVersion>
    {
        public DatabaseVersionMap()
        {
            // Primary Key
            this.HasKey(t => t.BuildNumber);

            // Properties
            this.Property(t => t.BuildNumber)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.SystemVersion)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.AddedBy)
                .IsRequired()
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("DatabaseVersion");
            this.Property(t => t.BuildNumber).HasColumnName("BuildNumber");
            this.Property(t => t.SystemVersion).HasColumnName("SystemVersion");
            this.Property(t => t.AddedDate).HasColumnName("AddedDate");
            this.Property(t => t.AddedBy).HasColumnName("AddedBy");
        }
    }
}
