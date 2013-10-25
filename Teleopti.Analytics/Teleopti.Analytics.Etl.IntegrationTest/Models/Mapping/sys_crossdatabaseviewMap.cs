using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class sys_crossdatabaseviewMap : EntityTypeConfiguration<sys_crossdatabaseview>
    {
        public sys_crossdatabaseviewMap()
        {
            // Primary Key
            this.HasKey(t => t.view_id);

            // Properties
            this.Property(t => t.view_name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.view_definition)
                .IsRequired()
                .HasMaxLength(4000);

            // Table & Column Mappings
            this.ToTable("sys_crossdatabaseview", "mart");
            this.Property(t => t.view_id).HasColumnName("view_id");
            this.Property(t => t.view_name).HasColumnName("view_name");
            this.Property(t => t.view_definition).HasColumnName("view_definition");
            this.Property(t => t.target_id).HasColumnName("target_id");

            // Relationships
            this.HasRequired(t => t.sys_crossdatabaseview_target)
                .WithMany(t => t.sys_crossdatabaseview)
                .HasForeignKey(d => d.target_id);

        }
    }
}
