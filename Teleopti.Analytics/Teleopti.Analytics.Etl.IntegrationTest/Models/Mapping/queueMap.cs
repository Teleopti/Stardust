using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class queueMap : EntityTypeConfiguration<queue>
    {
        public queueMap()
        {
            // Primary Key
            this.HasKey(t => t.queue1);

            // Properties
            this.Property(t => t.orig_desc)
                .HasMaxLength(50);

            this.Property(t => t.display_desc)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("queues");
            this.Property(t => t.queue1).HasColumnName("queue");
            this.Property(t => t.orig_desc).HasColumnName("orig_desc");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.orig_queue_id).HasColumnName("orig_queue_id");
            this.Property(t => t.display_desc).HasColumnName("display_desc");

            // Relationships
            this.HasRequired(t => t.log_object)
                .WithMany(t => t.queues)
                .HasForeignKey(d => d.log_object_id);

        }
    }
}
