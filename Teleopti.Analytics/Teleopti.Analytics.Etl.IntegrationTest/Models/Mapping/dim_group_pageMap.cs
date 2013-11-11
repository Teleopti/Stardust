using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_group_pageMap : EntityTypeConfiguration<dim_group_page>
    {
        public dim_group_pageMap()
        {
            // Primary Key
            this.HasKey(t => t.group_page_id);

            // Properties
            this.Property(t => t.group_page_name)
                .HasMaxLength(100);

            this.Property(t => t.group_page_name_resource_key)
                .HasMaxLength(100);

            this.Property(t => t.group_name)
                .IsRequired()
                .HasMaxLength(1024);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("dim_group_page", "mart");
            this.Property(t => t.group_page_id).HasColumnName("group_page_id");
            this.Property(t => t.group_page_code).HasColumnName("group_page_code");
            this.Property(t => t.group_page_name).HasColumnName("group_page_name");
            this.Property(t => t.group_page_name_resource_key).HasColumnName("group_page_name_resource_key");
            this.Property(t => t.group_id).HasColumnName("group_id");
            this.Property(t => t.group_code).HasColumnName("group_code");
            this.Property(t => t.group_name).HasColumnName("group_name");
            this.Property(t => t.group_is_custom).HasColumnName("group_is_custom");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
