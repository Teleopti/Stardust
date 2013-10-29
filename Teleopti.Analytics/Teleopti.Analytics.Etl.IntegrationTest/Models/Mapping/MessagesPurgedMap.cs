using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class MessagesPurgedMap : EntityTypeConfiguration<MessagesPurged>
    {
        public MessagesPurgedMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Headers)
                .HasMaxLength(2000);

            // Table & Column Mappings
            this.ToTable("MessagesPurged", "Queue");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.PurgedAt).HasColumnName("PurgedAt");
            this.Property(t => t.MessageId).HasColumnName("MessageId");
            this.Property(t => t.QueueId).HasColumnName("QueueId");
            this.Property(t => t.CreatedAt).HasColumnName("CreatedAt");
            this.Property(t => t.ProcessingUntil).HasColumnName("ProcessingUntil");
            this.Property(t => t.ExpiresAt).HasColumnName("ExpiresAt");
            this.Property(t => t.Processed).HasColumnName("Processed");
            this.Property(t => t.Headers).HasColumnName("Headers");
            this.Property(t => t.Payload).HasColumnName("Payload");
            this.Property(t => t.ProcessedCount).HasColumnName("ProcessedCount");
        }
    }
}
