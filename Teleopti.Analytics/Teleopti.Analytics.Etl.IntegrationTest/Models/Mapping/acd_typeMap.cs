using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class acd_typeMap : EntityTypeConfiguration<acd_type>
    {
        public acd_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.acd_type_id);

            // Properties
            this.Property(t => t.acd_type_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.acd_type_desc)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("acd_type");
            this.Property(t => t.acd_type_id).HasColumnName("acd_type_id");
            this.Property(t => t.acd_type_desc).HasColumnName("acd_type_desc");
        }
    }
}
