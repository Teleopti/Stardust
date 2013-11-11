using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class fact_requestMap : EntityTypeConfiguration<fact_request>
    {
        public fact_requestMap()
        {
            // Primary Key
            this.HasKey(t => t.request_code);

            // Properties
            // Table & Column Mappings
            this.ToTable("fact_request", "mart");
            this.Property(t => t.request_code).HasColumnName("request_code");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.request_start_date_id).HasColumnName("request_start_date_id");
            this.Property(t => t.application_datetime).HasColumnName("application_datetime");
            this.Property(t => t.request_startdate).HasColumnName("request_startdate");
            this.Property(t => t.request_enddate).HasColumnName("request_enddate");
            this.Property(t => t.request_type_id).HasColumnName("request_type_id");
            this.Property(t => t.request_status_id).HasColumnName("request_status_id");
            this.Property(t => t.request_day_count).HasColumnName("request_day_count");
            this.Property(t => t.request_start_date_count).HasColumnName("request_start_date_count");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.absence_id).HasColumnName("absence_id");
            this.Property(t => t.request_starttime).HasColumnName("request_starttime");
            this.Property(t => t.request_endtime).HasColumnName("request_endtime");
            this.Property(t => t.requested_time_m).HasColumnName("requested_time_m");

            // Relationships
            this.HasRequired(t => t.dim_absence)
                .WithMany(t => t.fact_request)
                .HasForeignKey(d => d.absence_id);
            this.HasRequired(t => t.dim_date)
                .WithMany(t => t.fact_request)
                .HasForeignKey(d => d.request_start_date_id);
            this.HasRequired(t => t.dim_person)
                .WithMany(t => t.fact_request)
                .HasForeignKey(d => d.person_id);
            this.HasRequired(t => t.dim_request_status)
                .WithMany(t => t.fact_request)
                .HasForeignKey(d => d.request_status_id);
            this.HasRequired(t => t.dim_request_type)
                .WithMany(t => t.fact_request)
                .HasForeignKey(d => d.request_type_id);

        }
    }
}
