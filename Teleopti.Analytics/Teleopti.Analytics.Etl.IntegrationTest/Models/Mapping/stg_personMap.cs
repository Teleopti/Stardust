using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_personMap : EntityTypeConfiguration<stg_person>
    {
        public stg_personMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_code, t.valid_from_date });

            // Properties
            this.Property(t => t.person_name)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.person_first_name)
                .IsRequired()
                .HasMaxLength(25);

            this.Property(t => t.person_last_name)
                .IsRequired()
                .HasMaxLength(25);

            this.Property(t => t.team_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.site_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.business_unit_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.email)
                .HasMaxLength(200);

            this.Property(t => t.note)
                .IsFixedLength()
                .HasMaxLength(1024);

            this.Property(t => t.employment_number)
                .HasMaxLength(50);

            this.Property(t => t.time_zone_code)
                .HasMaxLength(50);

            this.Property(t => t.contract_name)
                .HasMaxLength(50);

            this.Property(t => t.parttime_percentage)
                .HasMaxLength(50);

            this.Property(t => t.employment_type)
                .HasMaxLength(50);

            this.Property(t => t.windows_domain)
                .HasMaxLength(50);

            this.Property(t => t.windows_username)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_person", "stage");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.valid_from_date).HasColumnName("valid_from_date");
            this.Property(t => t.valid_to_date).HasColumnName("valid_to_date");
            this.Property(t => t.valid_from_interval_id).HasColumnName("valid_from_interval_id");
            this.Property(t => t.valid_to_interval_id).HasColumnName("valid_to_interval_id");
            this.Property(t => t.valid_to_interval_start).HasColumnName("valid_to_interval_start");
            this.Property(t => t.person_period_code).HasColumnName("person_period_code");
            this.Property(t => t.person_name).HasColumnName("person_name");
            this.Property(t => t.person_first_name).HasColumnName("person_first_name");
            this.Property(t => t.person_last_name).HasColumnName("person_last_name");
            this.Property(t => t.team_code).HasColumnName("team_code");
            this.Property(t => t.team_name).HasColumnName("team_name");
            this.Property(t => t.scorecard_code).HasColumnName("scorecard_code");
            this.Property(t => t.site_code).HasColumnName("site_code");
            this.Property(t => t.site_name).HasColumnName("site_name");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.email).HasColumnName("email");
            this.Property(t => t.note).HasColumnName("note");
            this.Property(t => t.employment_number).HasColumnName("employment_number");
            this.Property(t => t.employment_start_date).HasColumnName("employment_start_date");
            this.Property(t => t.employment_end_date).HasColumnName("employment_end_date");
            this.Property(t => t.time_zone_code).HasColumnName("time_zone_code");
            this.Property(t => t.is_agent).HasColumnName("is_agent");
            this.Property(t => t.is_user).HasColumnName("is_user");
            this.Property(t => t.contract_code).HasColumnName("contract_code");
            this.Property(t => t.contract_name).HasColumnName("contract_name");
            this.Property(t => t.parttime_code).HasColumnName("parttime_code");
            this.Property(t => t.parttime_percentage).HasColumnName("parttime_percentage");
            this.Property(t => t.employment_type).HasColumnName("employment_type");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.windows_domain).HasColumnName("windows_domain");
            this.Property(t => t.windows_username).HasColumnName("windows_username");
        }
    }
}
