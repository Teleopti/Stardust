using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class ccc_system_infoMap : EntityTypeConfiguration<ccc_system_info>
    {
        public ccc_system_infoMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.desc)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.varchar_value)
                .IsFixedLength()
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("ccc_system_info");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.desc).HasColumnName("desc");
            this.Property(t => t.int_value).HasColumnName("int_value");
            this.Property(t => t.varchar_value).HasColumnName("varchar_value");
        }
    }
}
