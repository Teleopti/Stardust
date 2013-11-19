using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_requested_daysMap : EntityTypeConfiguration<fact_requested_days>
    {
        public fact_requested_daysMap()
        {
            // Primary Key
            this.HasKey(t => new { t.request_code, t.request_date_id });

            // Properties
            this.Property(t => t.request_date_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("fact_requested_days", "mart");
            this.Property(t => t.request_code).HasColumnName("request_code");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.request_date_id).HasColumnName("request_date_id");
            this.Property(t => t.request_type_id).HasColumnName("request_type_id");
            this.Property(t => t.request_status_id).HasColumnName("request_status_id");
            this.Property(t => t.request_day_count).HasColumnName("request_day_count");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.absence_id).HasColumnName("absence_id");

            // Relationships
            this.HasOptional(t => t.dim_absence)
                .WithMany(t => t.fact_requested_days)
                .HasForeignKey(d => d.absence_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_requested_days)
                .HasForeignKey(d => d.request_date_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_requested_days)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_request_status)
                .WithMany(t => t.fact_requested_days)
                .HasForeignKey(d => d.request_status_id);
            this.HasRequired(t => t.dim_request_type)
                .WithMany(t => t.fact_requested_days)
                .HasForeignKey(d => d.request_type_id);

        }
    }
}
