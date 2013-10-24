using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class sys_configurationMap : EntityTypeConfiguration<sys_configuration>
    {
        public sys_configurationMap()
        {
            // Primary Key
            this.HasKey(t => t.key);

            // Properties
            this.Property(t => t.key)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.value)
                .IsRequired()
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("sys_configuration", "mart");
            this.Property(t => t.key).HasColumnName("key");
            this.Property(t => t.value).HasColumnName("value");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
        }
    }
}
