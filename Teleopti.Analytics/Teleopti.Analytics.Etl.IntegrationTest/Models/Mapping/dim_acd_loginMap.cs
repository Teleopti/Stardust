using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_acd_loginMap : EntityTypeConfiguration<dim_acd_login>
    {
        public dim_acd_loginMap()
        {
            // Primary Key
            this.HasKey(t => t.acd_login_id);

            // Properties
            this.Property(t => t.acd_login_original_id)
                .HasMaxLength(50);

            this.Property(t => t.acd_login_name)
                .HasMaxLength(100);

            this.Property(t => t.log_object_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_acd_login", "mart");
            this.Property(t => t.acd_login_id).HasColumnName("acd_login_id");
            this.Property(t => t.acd_login_agg_id).HasColumnName("acd_login_agg_id");
            this.Property(t => t.acd_login_original_id).HasColumnName("acd_login_original_id");
            this.Property(t => t.acd_login_name).HasColumnName("acd_login_name");
            this.Property(t => t.log_object_name).HasColumnName("log_object_name");
            this.Property(t => t.is_active).HasColumnName("is_active");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
