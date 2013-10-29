using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_queueMap : EntityTypeConfiguration<dim_queue>
    {
        public dim_queueMap()
        {
            // Primary Key
            this.HasKey(t => t.queue_id);

            // Properties
            this.Property(t => t.queue_original_id)
                .HasMaxLength(50);

            this.Property(t => t.queue_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.queue_description)
                .HasMaxLength(100);

            this.Property(t => t.log_object_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_queue", "mart");
            this.Property(t => t.queue_id).HasColumnName("queue_id");
            this.Property(t => t.queue_agg_id).HasColumnName("queue_agg_id");
            this.Property(t => t.queue_original_id).HasColumnName("queue_original_id");
            this.Property(t => t.queue_name).HasColumnName("queue_name");
            this.Property(t => t.queue_description).HasColumnName("queue_description");
            this.Property(t => t.log_object_name).HasColumnName("log_object_name");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
