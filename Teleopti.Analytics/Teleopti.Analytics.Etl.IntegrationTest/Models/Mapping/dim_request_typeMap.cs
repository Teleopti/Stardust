using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_request_typeMap : EntityTypeConfiguration<dim_request_type>
    {
        public dim_request_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.request_type_id);

            // Properties
            this.Property(t => t.request_type_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.resource_key)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_request_type", "mart");
            this.Property(t => t.request_type_id).HasColumnName("request_type_id");
            this.Property(t => t.request_type_name).HasColumnName("request_type_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
