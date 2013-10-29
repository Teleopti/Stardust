using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class bridge_queue_workloadMap : EntityTypeConfiguration<bridge_queue_workload>
    {
        public bridge_queue_workloadMap()
        {
            // Primary Key
            this.HasKey(t => new { t.queue_id, t.workload_id });

            // Properties
            this.Property(t => t.queue_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.workload_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("bridge_queue_workload", "mart");
            this.Property(t => t.queue_id).HasColumnName("queue_id");
            this.Property(t => t.workload_id).HasColumnName("workload_id");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_queue)
                .WithMany(t => t.bridge_queue_workload)
                .HasForeignKey(d => d.queue_id);
            this.HasOptional(t => t.dim_skill)
                .WithMany(t => t.bridge_queue_workload)
                .HasForeignKey(d => d.skill_id);
            this.HasRequired(t => t.dim_workload)
                .WithMany(t => t.bridge_queue_workload)
                .HasForeignKey(d => d.workload_id);

        }
    }
}
