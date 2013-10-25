using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_schedule_day_countMap : EntityTypeConfiguration<fact_schedule_day_count>
    {
        public fact_schedule_day_countMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.start_interval_id, t.person_id, t.scenario_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.start_interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_schedule_day_count", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.start_interval_id).HasColumnName("start_interval_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.starttime).HasColumnName("starttime");
            this.Property(t => t.shift_category_id).HasColumnName("shift_category_id");
            this.Property(t => t.day_off_id).HasColumnName("day_off_id");
            this.Property(t => t.absence_id).HasColumnName("absence_id");
            this.Property(t => t.day_count).HasColumnName("day_count");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");

            // Relationships
            this.HasRequired(t => t.dim_absence)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.absence_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_day_off)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.day_off_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.start_interval_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.scenario_id);
            this.HasRequired(t => t.dim_shift_category)
                .WithMany(t => t.fact_schedule_day_count)
                .HasForeignKey(d => d.shift_category_id);

        }
    }
}
