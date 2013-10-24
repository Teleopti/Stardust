using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_preference_typeMap : EntityTypeConfiguration<dim_preference_type>
    {
        public dim_preference_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.preference_type_id);

            // Properties
            this.Property(t => t.preference_type_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.preference_type_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.resource_key)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_preference_type", "mart");
            this.Property(t => t.preference_type_id).HasColumnName("preference_type_id");
            this.Property(t => t.preference_type_name).HasColumnName("preference_type_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
