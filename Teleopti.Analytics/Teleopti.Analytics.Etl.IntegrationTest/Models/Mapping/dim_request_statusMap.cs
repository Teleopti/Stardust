using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_request_statusMap : EntityTypeConfiguration<dim_request_status>
    {
        public dim_request_statusMap()
        {
            // Primary Key
            this.HasKey(t => t.request_status_id);

            // Properties
            this.Property(t => t.request_status_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.resource_key)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_request_status", "mart");
            this.Property(t => t.request_status_id).HasColumnName("request_status_id");
            this.Property(t => t.request_status_name).HasColumnName("request_status_name");
            this.Property(t => t.resource_key).HasColumnName("resource_key");
        }
    }
}
