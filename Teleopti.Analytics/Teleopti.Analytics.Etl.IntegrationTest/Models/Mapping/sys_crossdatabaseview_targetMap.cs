using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class sys_crossdatabaseview_targetMap : EntityTypeConfiguration<sys_crossdatabaseview_target>
    {
        public sys_crossdatabaseview_targetMap()
        {
            // Primary Key
            this.HasKey(t => t.target_id);

            // Properties
            this.Property(t => t.target_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.target_customName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.target_defaultName)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("sys_crossdatabaseview_target", "mart");
            this.Property(t => t.target_id).HasColumnName("target_id");
            this.Property(t => t.target_customName).HasColumnName("target_customName");
            this.Property(t => t.target_defaultName).HasColumnName("target_defaultName");
            this.Property(t => t.confirmed).HasColumnName("confirmed");
        }
    }
}
