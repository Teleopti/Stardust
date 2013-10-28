using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class report_controlMap : EntityTypeConfiguration<report_control>
    {
        public report_controlMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.control_name)
                .HasMaxLength(50);

            this.Property(t => t.fill_proc_name)
                .IsRequired()
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("report_control", "mart");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.control_id).HasColumnName("control_id");
            this.Property(t => t.control_name).HasColumnName("control_name");
            this.Property(t => t.fill_proc_name).HasColumnName("fill_proc_name");
        }
    }
}
