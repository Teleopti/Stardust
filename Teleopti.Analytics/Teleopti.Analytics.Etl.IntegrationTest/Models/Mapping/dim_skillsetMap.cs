using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_skillsetMap : EntityTypeConfiguration<dim_skillset>
    {
        public dim_skillsetMap()
        {
            // Primary Key
            this.HasKey(t => t.skillset_id);

            // Properties
            this.Property(t => t.skillset_code)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.skillset_name)
                .IsRequired()
                .HasMaxLength(4000);

            // Table & Column Mappings
            this.ToTable("dim_skillset", "mart");
            this.Property(t => t.skillset_id).HasColumnName("skillset_id");
            this.Property(t => t.skillset_code).HasColumnName("skillset_code");
            this.Property(t => t.skillset_name).HasColumnName("skillset_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
