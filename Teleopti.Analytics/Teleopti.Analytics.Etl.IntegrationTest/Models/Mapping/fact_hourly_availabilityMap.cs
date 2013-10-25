using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_hourly_availabilityMap : EntityTypeConfiguration<fact_hourly_availability>
    {
        public fact_hourly_availabilityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.person_id, t.scenario_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_hourly_availability", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.available_time_m).HasColumnName("available_time_m");
            this.Property(t => t.available_days).HasColumnName("available_days");
            this.Property(t => t.scheduled_time_m).HasColumnName("scheduled_time_m");
            this.Property(t => t.scheduled_days).HasColumnName("scheduled_days");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");

            // Relationships
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_hourly_availability)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_hourly_availability)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_hourly_availability)
                .HasForeignKey(d => d.scenario_id);

        }
    }
}
