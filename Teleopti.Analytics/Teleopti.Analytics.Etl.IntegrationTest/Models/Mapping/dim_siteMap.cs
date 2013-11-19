using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_siteMap : EntityTypeConfiguration<dim_site>
    {
        public dim_siteMap()
        {
            // Primary Key
            this.HasKey(t => t.site_id);

            // Properties
            this.Property(t => t.site_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_site", "mart");
            this.Property(t => t.site_id).HasColumnName("site_id");
            this.Property(t => t.site_code).HasColumnName("site_code");
            this.Property(t => t.site_name).HasColumnName("site_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasOptional(t => t.dim_business_unit)
                .WithMany(t => t.dim_site)
                .HasForeignKey(d => d.business_unit_id);

        }
    }
}
