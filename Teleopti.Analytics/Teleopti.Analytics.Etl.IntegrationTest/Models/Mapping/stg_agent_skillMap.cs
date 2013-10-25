using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_agent_skillMap : EntityTypeConfiguration<stg_agent_skill>
    {
        public stg_agent_skillMap()
        {
            // Primary Key
            this.HasKey(t => new { t.skill_date, t.interval_id, t.person_code, t.skill_code });

            // Properties
            this.Property(t => t.interval_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("stg_agent_skill", "stage");
            this.Property(t => t.skill_date).HasColumnName("skill_date");
            this.Property(t => t.interval_id).HasColumnName("interval_id");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.skill_code).HasColumnName("skill_code");
            this.Property(t => t.date_from).HasColumnName("date_from");
            this.Property(t => t.date_to).HasColumnName("date_to");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
        }
    }
}
