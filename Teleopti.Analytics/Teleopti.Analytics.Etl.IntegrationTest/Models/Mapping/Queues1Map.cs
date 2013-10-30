using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class Queues1Map : EntityTypeConfiguration<Queues1>
    {
        public Queues1Map()
        {
            // Primary Key
            this.HasKey(t => t.QueueId);

            // Properties
            this.Property(t => t.QueueName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Endpoint)
                .IsRequired()
                .HasMaxLength(250);

            // Table & Column Mappings
            this.ToTable("Queues", "Queue");
            this.Property(t => t.QueueName).HasColumnName("QueueName");
            this.Property(t => t.QueueId).HasColumnName("QueueId");
            this.Property(t => t.ParentQueueId).HasColumnName("ParentQueueId");
            this.Property(t => t.Endpoint).HasColumnName("Endpoint");
        }
    }
}
