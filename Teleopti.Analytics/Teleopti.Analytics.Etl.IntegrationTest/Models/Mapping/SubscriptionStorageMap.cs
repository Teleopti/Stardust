using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class SubscriptionStorageMap : EntityTypeConfiguration<SubscriptionStorage>
    {
        public SubscriptionStorageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Key)
                .IsRequired()
                .HasMaxLength(250);

            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("SubscriptionStorage", "Queue");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Key).HasColumnName("Key");
            this.Property(t => t.Value).HasColumnName("Value");
        }
    }
}
