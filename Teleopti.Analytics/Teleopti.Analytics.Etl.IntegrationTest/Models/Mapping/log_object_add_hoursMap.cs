using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class log_object_add_hoursMap : EntityTypeConfiguration<log_object_add_hours>
    {
        public log_object_add_hoursMap()
        {
            // Primary Key
            this.HasKey(t => new { t.log_object_id, t.datetime_from });

            // Properties
            this.Property(t => t.log_object_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("log_object_add_hours");
            this.Property(t => t.log_object_id).HasColumnName("log_object_id");
            this.Property(t => t.datetime_from).HasColumnName("datetime_from");
            this.Property(t => t.datetime_to).HasColumnName("datetime_to");
            this.Property(t => t.add_hours).HasColumnName("add_hours");

            // Relationships
            this.HasRequired(t => t.log_object)
                .WithMany(t => t.log_object_add_hours)
                .HasForeignKey(d => d.log_object_id);

        }
    }
}
