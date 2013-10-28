using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class period_typeMap : EntityTypeConfiguration<period_type>
    {
        public period_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.period_type_id);

            // Properties
            this.Property(t => t.period_type_name)
                .HasMaxLength(100);

            this.Property(t => t.resource_key)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("period_type", "mart");
            this.Property(t => t.period_type_id).HasColumnName("period_type_id");
            this.Property(t => t.period_type_name).HasColumnName("period_type_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
