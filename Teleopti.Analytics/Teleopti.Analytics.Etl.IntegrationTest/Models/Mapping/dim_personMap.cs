using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_personMap : EntityTypeConfiguration<dim_person>
    {
        public dim_personMap()
        {
            // Primary Key
            this.HasKey(t => t.person_id);

            // Properties
            this.Property(t => t.person_name)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.first_name)
                .IsRequired()
                .HasMaxLength(30);

            this.Property(t => t.last_name)
                .IsRequired()
                .HasMaxLength(30);

            this.Property(t => t.employment_number)
                .HasMaxLength(50);

            this.Property(t => t.employment_type_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.contract_name)
                .HasMaxLength(50);

            this.Property(t => t.parttime_percentage)
                .HasMaxLength(50);

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
                .HasMaxLength(1024);

            this.Property(t => t.windows_domain)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.windows_username)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("dim_person", "mart");
            this.Property(t => t.person_id).HasColumnName("person_id");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.valid_from_date).HasColumnName("valid_from_date");
            this.Property(t => t.valid_to_date).HasColumnName("valid_to_date");
            this.Property(t => t.valid_from_date_id).HasColumnName("valid_from_date_id");
            this.Property(t => t.valid_from_interval_id).HasColumnName("valid_from_interval_id");
            this.Property(t => t.valid_to_date_id).HasColumnName("valid_to_date_id");
            this.Property(t => t.valid_to_interval_id).HasColumnName("valid_to_interval_id");
            this.Property(t => t.person_period_code).HasColumnName("person_period_code");
            this.Property(t => t.person_name).HasColumnName("person_name");
            this.Property(t => t.first_name).HasColumnName("first_name");
            this.Property(t => t.last_name).HasColumnName("last_name");
            this.Property(t => t.employment_number).HasColumnName("employment_number");
            this.Property(t => t.employment_type_code).HasColumnName("employment_type_code");
            this.Property(t => t.employment_type_name).HasColumnName("employment_type_name");
            this.Property(t => t.contract_code).HasColumnName("contract_code");
            this.Property(t => t.contract_name).HasColumnName("contract_name");
            this.Property(t => t.parttime_code).HasColumnName("parttime_code");
            this.Property(t => t.parttime_percentage).HasColumnName("parttime_percentage");
            this.Property(t => t.team_id).HasColumnName("team_id");
            this.Property(t => t.team_code).HasColumnName("team_code");
            this.Property(t => t.team_name).HasColumnName("team_name");
            this.Property(t => t.site_id).HasColumnName("site_id");
            this.Property(t => t.site_code).HasColumnName("site_code");
            this.Property(t => t.site_name).HasColumnName("site_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.business_unit_code).HasColumnName("business_unit_code");
            this.Property(t => t.business_unit_name).HasColumnName("business_unit_name");
            this.Property(t => t.skillset_id).HasColumnName("skillset_id");
            this.Property(t => t.email).HasColumnName("email");
            this.Property(t => t.note).HasColumnName("note");
            this.Property(t => t.employment_start_date).HasColumnName("employment_start_date");
            this.Property(t => t.employment_end_date).HasColumnName("employment_end_date");
            this.Property(t => t.time_zone_id).HasColumnName("time_zone_id");
            this.Property(t => t.is_agent).HasColumnName("is_agent");
            this.Property(t => t.is_user).HasColumnName("is_user");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.to_be_deleted).HasColumnName("to_be_deleted");
            this.Property(t => t.windows_domain).HasColumnName("windows_domain");
            this.Property(t => t.windows_username).HasColumnName("windows_username");

            // Relationships
            this.HasOptional(t => t.dim_skillset)
                .WithMany(t => t.dim_person)
                .HasForeignKey(d => d.skillset_id);

        }
    }
}
