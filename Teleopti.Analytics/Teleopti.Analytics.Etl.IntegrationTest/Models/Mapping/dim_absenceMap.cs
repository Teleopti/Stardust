using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_absenceMap : EntityTypeConfiguration<dim_absence>
    {
        public dim_absenceMap()
        {
            // Primary Key
            this.HasKey(t => t.absence_id);

            // Properties
            this.Property(t => t.absence_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.in_contract_time_name)
                .HasMaxLength(50);

            this.Property(t => t.in_paid_time_name)
                .HasMaxLength(50);

            this.Property(t => t.in_work_time_name)
                .HasMaxLength(50);

            this.Property(t => t.display_color_html)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(7);

            this.Property(t => t.absence_shortname)
                .IsRequired()
                .HasMaxLength(25);

            // Table & Column Mappings
            this.ToTable("dim_absence", "mart");
            this.Property(t => t.absence_id).HasColumnName("absence_id");
            this.Property(t => t.absence_code).HasColumnName("absence_code");
            this.Property(t => t.absence_name).HasColumnName("absence_name");
            this.Property(t => t.display_color).HasColumnName("display_color");
            this.Property(t => t.in_contract_time).HasColumnName("in_contract_time");
            this.Property(t => t.in_contract_time_name).HasColumnName("in_contract_time_name");
            this.Property(t => t.in_paid_time).HasColumnName("in_paid_time");
            this.Property(t => t.in_paid_time_name).HasColumnName("in_paid_time_name");
            this.Property(t => t.in_work_time).HasColumnName("in_work_time");
            this.Property(t => t.in_work_time_name).HasColumnName("in_work_time_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
            this.Property(t => t.is_deleted).HasColumnName("is_deleted");
            this.Property(t => t.display_color_html).HasColumnName("display_color_html");
            this.Property(t => t.absence_shortname).HasColumnName("absence_shortname");
        }
    }
}
