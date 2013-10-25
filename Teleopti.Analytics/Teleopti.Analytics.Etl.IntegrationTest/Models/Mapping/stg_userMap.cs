using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_userMap : EntityTypeConfiguration<stg_user>
    {
        public stg_userMap()
        {
            // Primary Key
            this.HasKey(t => t.person_code);

            // Properties
            this.Property(t => t.person_first_name)
                .IsRequired()
                .HasMaxLength(25);

            this.Property(t => t.person_last_name)
                .IsRequired()
                .HasMaxLength(25);

            this.Property(t => t.application_logon_name)
                .HasMaxLength(50);

            this.Property(t => t.windows_logon_name)
                .HasMaxLength(50);

            this.Property(t => t.windows_domain_name)
                .HasMaxLength(50);

            this.Property(t => t.password)
                .HasMaxLength(50);

            this.Property(t => t.email)
                .HasMaxLength(200);

            this.Property(t => t.language_name)
                .HasMaxLength(50);

            this.Property(t => t.culture)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stg_user", "stage");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.person_first_name).HasColumnName("person_first_name");
            this.Property(t => t.person_last_name).HasColumnName("person_last_name");
            this.Property(t => t.application_logon_name).HasColumnName("application_logon_name");
            this.Property(t => t.windows_logon_name).HasColumnName("windows_logon_name");
            this.Property(t => t.windows_domain_name).HasColumnName("windows_domain_name");
            this.Property(t => t.password).HasColumnName("password");
            this.Property(t => t.email).HasColumnName("email");
            this.Property(t => t.language_id).HasColumnName("language_id");
            this.Property(t => t.language_name).HasColumnName("language_name");
            this.Property(t => t.culture).HasColumnName("culture");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
