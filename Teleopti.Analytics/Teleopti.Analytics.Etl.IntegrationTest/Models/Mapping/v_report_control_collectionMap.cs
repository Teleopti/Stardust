using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class v_report_control_collectionMap : EntityTypeConfiguration<v_report_control_collection>
    {
        public v_report_control_collectionMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Id, t.CollectionId, t.ControlId, t.print_order, t.default_value, t.control_name_resource_key });

            // Properties
            this.Property(t => t.print_order)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.default_value)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.control_name_resource_key)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.fill_proc_param)
                .HasMaxLength(100);

            this.Property(t => t.param_name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("v_report_control_collection", "mart");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.CollectionId).HasColumnName("CollectionId");
            this.Property(t => t.ControlId).HasColumnName("ControlId");
            this.Property(t => t.print_order).HasColumnName("print_order");
            this.Property(t => t.default_value).HasColumnName("default_value");
            this.Property(t => t.control_name_resource_key).HasColumnName("control_name_resource_key");
            this.Property(t => t.fill_proc_param).HasColumnName("fill_proc_param");
            this.Property(t => t.param_name).HasColumnName("param_name");
            this.Property(t => t.DependOf1).HasColumnName("DependOf1");
            this.Property(t => t.DependOf2).HasColumnName("DependOf2");
            this.Property(t => t.DependOf3).HasColumnName("DependOf3");
            this.Property(t => t.DependOf4).HasColumnName("DependOf4");
        }
    }
}
