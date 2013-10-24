using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class log_object_detailMap : EntityTypeConfiguration<log_object_detail>
    {
        public log_object_detailMap()
        {
            // Primary Key
            this.HasKey(t => new { t.log_object_id, t.detail_id });

            // Properties
            this.Property(t => t.log_object_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.detail_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.detail_desc)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.proc_name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("log_object_detail");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.detail_id).HasColumnName("detail_id");
            this.Property(t => t.detail_desc).HasColumnName("detail_desc");
            this.Property(t => t.proc_name).HasColumnName("proc_name");
            this.Property(t => t.int_value).HasColumnName("int_value");
            this.Property(t => t.date_value).HasColumnName("date_value");

            // Relationships
            this.HasRequired(t => t.log_object)
                .WithMany(t => t.log_object_detail)
                .HasForeignKey(d => d.log_object_id);

        }
    }
}
