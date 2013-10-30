using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class log_objectMap : EntityTypeConfiguration<log_object>
    {
        public log_objectMap()
        {
            // Primary Key
            this.HasKey(t => t.log_object_id);

            // Properties
            this.Property(t => t.log_object_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.log_object_desc)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.logDB_name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("log_object");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.acd_type_id).HasColumnName("acd_type_id");
            this.Property(t => t.log_object_desc).HasColumnName("log_object_desc");
            this.Property(t => t.logDB_name).HasColumnName("logDB_name");
            this.Property(t => t.intervals_per_day).HasColumnName("intervals_per_day");
            this.Property(t => t.default_service_level_sec).HasColumnName("default_service_level_sec");
            this.Property(t => t.default_short_call_treshold).HasColumnName("default_short_call_treshold");

            // Relationships
            this.HasRequired(t => t.acd_type)
                .WithMany(t => t.log_object)
                .HasForeignKey(d => d.acd_type_id);

        }
    }
}
