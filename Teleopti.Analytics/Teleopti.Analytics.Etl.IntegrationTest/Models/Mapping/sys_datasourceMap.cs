using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class sys_datasourceMap : EntityTypeConfiguration<sys_datasource>
    {
        public sys_datasourceMap()
        {
            // Primary Key
            this.HasKey(t => t.datasource_id);

            // Properties
            this.Property(t => t.datasource_name)
                .HasMaxLength(100);

            this.Property(t => t.log_object_name)
                .HasMaxLength(100);

            this.Property(t => t.datasource_database_name)
                .HasMaxLength(100);

            this.Property(t => t.datasource_type_name)
                .HasMaxLength(100);

            this.Property(t => t.source_id)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("sys_datasource", "mart");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.datasource_name).HasColumnName("datasource_name");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.log_object_name).HasColumnName("log_object_name");
            this.Property(t => t.datasource_database_id).HasColumnName("datasource_database_id");
            this.Property(t => t.datasource_database_name).HasColumnName("datasource_database_name");
            this.Property(t => t.datasource_type_name).HasColumnName("datasource_type_name");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.inactive).HasColumnName("inactive");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.source_id).HasColumnName("source_id");
            //this.Property(t => t.internal).HasColumnName("internal");
        }
    }
}
