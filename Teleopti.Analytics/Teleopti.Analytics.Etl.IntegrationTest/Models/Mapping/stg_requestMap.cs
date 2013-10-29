using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_requestMap : EntityTypeConfiguration<stg_request>
    {
        public stg_requestMap()
        {
            // Primary Key
            this.HasKey(t => new { t.request_code, t.person_code, t.request_date, t.request_type_code, t.request_status_code });

            // Properties
            // Table & Column Mappings
            this.ToTable("stg_request", "stage");
            this.Property(t => t.request_code).HasColumnName("request_code");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.application_datetime).HasColumnName("application_datetime");
            this.Property(t => t.request_date).HasColumnName("request_date");
            this.Property(t => t.request_startdate).HasColumnName("request_startdate");
            this.Property(t => t.request_enddate).HasColumnName("request_enddate");
            this.Property(t => t.request_type_code).HasColumnName("request_type_code");
            this.Property(t => t.request_status_code).HasColumnName("request_status_code");
            this.Property(t => t.request_start_date_count).HasColumnName("request_start_date_count");
            this.Property(t => t.request_day_count).HasColumnName("request_day_count");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
            this.Property(t => t.request_starttime).HasColumnName("request_starttime");
            this.Property(t => t.request_endtime).HasColumnName("request_endtime");
            this.Property(t => t.absence_code).HasColumnName("absence_code");
        }
    }
}
