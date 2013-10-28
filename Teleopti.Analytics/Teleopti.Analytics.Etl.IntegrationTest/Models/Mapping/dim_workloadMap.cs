using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_workloadMap : EntityTypeConfiguration<dim_workload>
    {
        public dim_workloadMap()
        {
            // Primary Key
            this.HasKey(t => t.workload_id);

            // Properties
            this.Property(t => t.workload_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.skill_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.forecast_method_name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("dim_workload", "mart");
            this.Property(t => t.workload_id).HasColumnName("workload_id");
            this.Property(t => t.workload_code).HasColumnName("workload_code");
            this.Property(t => t.workload_name).HasColumnName("workload_name");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.skill_code).HasColumnName("skill_code");
            this.Property(t => t.skill_name).HasColumnName("skill_name");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.forecast_method_code).HasColumnName("forecast_method_code");
            this.Property(t => t.forecast_method_name).HasColumnName("forecast_method_name");
            this.Property(t => t.percentage_offered).HasColumnName("percentage_offered");
            this.Property(t => t.percentage_overflow_in).HasColumnName("percentage_overflow_in");
            this.Property(t => t.percentage_overflow_out).HasColumnName("percentage_overflow_out");
            this.Property(t => t.percentage_abandoned).HasColumnName("percentage_abandoned");
            this.Property(t => t.percentage_abandoned_short).HasColumnName("percentage_abandoned_short");
            this.Property(t => t.percentage_abandoned_within_service_level).HasColumnName("percentage_abandoned_within_service_level");
            this.Property(t => t.percentage_abandoned_after_service_level).HasColumnName("percentage_abandoned_after_service_level");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
        }
    }
}
