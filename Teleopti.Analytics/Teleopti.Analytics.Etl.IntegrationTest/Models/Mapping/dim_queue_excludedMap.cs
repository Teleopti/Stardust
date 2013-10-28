using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_queue_excludedMap : EntityTypeConfiguration<dim_queue_excluded>
    {
        public dim_queue_excludedMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue_original_id, t.datasource_id });

            // Properties
            this.Property(t => t.queue_original_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.datasource_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("dim_queue_excluded", "mart");
            this.Property(t => t.queue_original_id).HasColumnName("queue_original_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
        }
    }
}
