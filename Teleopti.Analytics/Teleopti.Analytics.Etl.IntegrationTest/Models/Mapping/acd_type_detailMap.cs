using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class acd_type_detailMap : EntityTypeConfiguration<acd_type_detail>
    {
        public acd_type_detailMap()
        {
            // Primary Key
            this.HasKey(t => new { t.acd_type_id, t.detail_id });

            // Properties
            this.Property(t => t.acd_type_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.detail_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.detail_name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.proc_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("acd_type_detail");
            this.Property(t => t.acd_type_id).HasColumnName("acd_type_id");
            this.Property(t => t.detail_id).HasColumnName("detail_id");
            this.Property(t => t.detail_name).HasColumnName("detail_name");
            this.Property(t => t.proc_name).HasColumnName("proc_name");

            // Relationships
            this.HasRequired(t => t.acd_type)
                .WithMany(t => t.acd_type_detail)
                .HasForeignKey(d => d.acd_type_id);

        }
    }
}
