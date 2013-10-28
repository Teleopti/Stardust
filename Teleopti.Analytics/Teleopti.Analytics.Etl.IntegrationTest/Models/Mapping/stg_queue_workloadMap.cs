using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_queue_workloadMap : EntityTypeConfiguration<stg_queue_workload>
    {
        public stg_queue_workloadMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue_code, t.workload_code, t.log_object_data_source_id, t.log_object_name });

            // Properties
            this.Property(t => t.queue_code)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.log_object_data_source_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.log_object_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_queue_workload", "stage");
            this.Property(t => t.queue_code).HasColumnName("queue_code");
            this.Property(t => t.workload_code).HasColumnName("workload_code");
            this.Property(t => t.log_object_data_source_id).HasColumnName("log_object_data_source_id");
            this.Property(t => t.log_object_name).HasColumnName("log_object_name");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
