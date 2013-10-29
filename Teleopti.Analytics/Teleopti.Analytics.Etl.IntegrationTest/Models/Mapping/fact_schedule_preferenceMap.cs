using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_schedule_preferenceMap : EntityTypeConfiguration<fact_schedule_preference>
    {
        public fact_schedule_preferenceMap()
        {
            // Primary Key
            this.HasKey(t => new { t.date_id, t.interval_id, t.person_id, t.scenario_id, t.preference_type_id, t.shift_category_id, t.day_off_id, t.absence_id });

            // Properties
            this.Property(t => t.date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.person_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.scenario_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.preference_type_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.shift_category_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.day_off_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.absence_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_schedule_preference", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.scenario_id).HasColumnName("scenario_id");
            this.Property(t => t.preference_type_id).HasColumnName("preference_type_id");
            this.Property(t => t.shift_category_id).HasColumnName("shift_category_id");
            this.Property(t => t.day_off_id).HasColumnName("day_off_id");
            this.Property(t => t.preferences_requested).HasColumnName("preferences_requested");
            this.Property(t => t.preferences_fulfilled).HasColumnName("preferences_fulfilled");
            this.Property(t => t.preferences_unfulfilled).HasColumnName("preferences_unfulfilled");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.must_haves).HasColumnName("must_haves");
            this.Property(t => t.absence_id).HasColumnName("absence_id");

            // Relationships
            this.HasRequired(t => t.dim_absence)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.absence_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.date_id);
            this.HasRequired(t => t.dim_day_off)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.day_off_id);
            this.HasRequired(t => t.dim_interval)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.interval_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_preference_type)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.preference_type_id);
            this.HasRequired(t => t.dim_scenario)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.scenario_id);
            this.HasRequired(t => t.dim_shift_category)
                .WithMany(t => t.fact_schedule_preference)
                .HasForeignKey(d => d.shift_category_id);

        }
    }
}
