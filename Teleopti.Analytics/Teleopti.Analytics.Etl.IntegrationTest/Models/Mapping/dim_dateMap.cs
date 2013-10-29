using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_dateMap : EntityTypeConfiguration<dim_date>
    {
        public dim_dateMap()
        {
            // Primary Key
            this.HasKey(t => t.date_id);

            // Properties
            this.Property(t => t.month_name)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.month_resource_key)
                .HasMaxLength(100);

            this.Property(t => t.weekday_name)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.weekday_resource_key)
                .HasMaxLength(100);

            this.Property(t => t.year_week)
                .IsRequired()
                .HasMaxLength(6);

            this.Property(t => t.quarter)
                .IsRequired()
                .HasMaxLength(6);

            // Table & Column Mappings
            this.ToTable("dim_date", "mart");
            this.Property(t => t.date_id).HasColumnName("date_id");
            this.Property(t => t.date_date).HasColumnName("date_date");
            this.Property(t => t.year).HasColumnName("year");
            this.Property(t => t.year_month).HasColumnName("year_month");
            this.Property(t => t.month).HasColumnName("month");
            this.Property(t => t.month_name).HasColumnName("month_name");
            this.Property(t => t.month_resource_key).HasColumnName("month_resource_key");
            this.Property(t => t.day_in_month).HasColumnName("day_in_month");
            this.Property(t => t.weekday_number).HasColumnName("weekday_number");
            this.Property(t => t.weekday_name).HasColumnName("weekday_name");
            this.Property(t => t.weekday_resource_key).HasColumnName("weekday_resource_key");
            this.Property(t => t.week_number).HasColumnName("week_number");
            this.Property(t => t.year_week).HasColumnName("year_week");
            this.Property(t => t.quarter).HasColumnName("quarter");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
        }
    }
}
