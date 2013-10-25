using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class dim_shift_lengthMap : EntityTypeConfiguration<dim_shift_length>
    {
        public dim_shift_lengthMap()
        {
            // Primary Key
            this.HasKey(t => t.shift_length_id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dim_shift_length", "mart");
            this.Property(t => t.shift_length_id).HasColumnName("shift_length_id");
            this.Property(t => t.shift_length_m).HasColumnName("shift_length_m");
            this.Property(t => t.shift_length_h).HasColumnName("shift_length_h");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
