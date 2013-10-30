using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class custom_reportMap : EntityTypeConfiguration<custom_report>
    {
        public custom_reportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.url)
                .HasMaxLength(500);

            this.Property(t => t.target)
                .HasMaxLength(50);

            this.Property(t => t.report_name)
                .HasMaxLength(500);

            this.Property(t => t.report_name_resource_key)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.rpt_file_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.proc_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.help_key)
                .HasMaxLength(500);

            this.Property(t => t.sub1_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.sub1_proc_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.sub2_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.sub2_proc_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("custom_report", "mart");
            this.Property(t => t.url).HasColumnName("url");
            this.Property(t => t.target).HasColumnName("target");
            this.Property(t => t.report_name).HasColumnName("report_name");
            this.Property(t => t.report_name_resource_key).HasColumnName("report_name_resource_key");
            this.Property(t => t.visible).HasColumnName("visible");
            this.Property(t => t.rpt_file_name).HasColumnName("rpt_file_name");
            this.Property(t => t.proc_name).HasColumnName("proc_name");
            this.Property(t => t.help_key).HasColumnName("help_key");
            this.Property(t => t.sub1_name).HasColumnName("sub1_name");
            this.Property(t => t.sub1_proc_name).HasColumnName("sub1_proc_name");
            this.Property(t => t.sub2_name).HasColumnName("sub2_name");
            this.Property(t => t.sub2_proc_name).HasColumnName("sub2_proc_name");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ControlCollectionId).HasColumnName("ControlCollectionId");
        }
    }
}
