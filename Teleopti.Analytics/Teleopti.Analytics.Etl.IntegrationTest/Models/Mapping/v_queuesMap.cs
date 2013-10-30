using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_queuesMap : EntityTypeConfiguration<v_queues>
    {
        public v_queuesMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue, t.log_object_id });

            // Properties
            this.Property(t => t.queue)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.orig_desc)
                .HasMaxLength(50);

            this.Property(t => t.log_object_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.display_desc)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("v_queues", "mart");
            this.Property(t => t.queue).HasColumnName("queue");
            this.Property(t => t.orig_desc).HasColumnName("orig_desc");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.orig_queue_id).HasColumnName("orig_queue_id");
            this.Property(t => t.display_desc).HasColumnName("display_desc");
        }
    }
}
