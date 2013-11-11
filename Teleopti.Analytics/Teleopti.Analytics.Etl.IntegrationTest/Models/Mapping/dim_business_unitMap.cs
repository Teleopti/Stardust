using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_business_unitMap : EntityTypeConfiguration<dim_business_unit>
    {
        public dim_business_unitMap()
        {
            // Primary Key
            this.HasKey(t => t.business_unit_id);

            // Properties
            this.Property(t => t.business_unit_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_business_unit", "mart");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
