using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class stg_agent_skillsetMap : EntityTypeConfiguration<stg_agent_skillset>
    {
        public stg_agent_skillsetMap()
        {
            // Primary Key
            this.HasKey(t => new { t.person_code, t.skill_id, t.date_from });

            // Properties
            this.Property(t => t.skill_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.skill_name)
                .HasMaxLength(100);

            this.Property(t => t.skill_sum_code)
                .HasMaxLength(4000);

            this.Property(t => t.skill_sum_name)
                .HasMaxLength(4000);

            // Table & Column Mappings
            this.ToTable("stg_agent_skillset", "stage");
            this.Property(t => t.person_code).HasColumnName("person_code");
            this.Property(t => t.skill_id).HasColumnName("skill_id");
            this.Property(t => t.date_from).HasColumnName("date_from");
            this.Property(t => t.skillset_id).HasColumnName("skillset_id");
            this.Property(t => t.date_to).HasColumnName("date_to");
            this.Property(t => t.skill_name).HasColumnName("skill_name");
            this.Property(t => t.skill_sum_code).HasColumnName("skill_sum_code");
            this.Property(t => t.skill_sum_name).HasColumnName("skill_sum_name");
            this.Property(t => t.business_unit_id).HasColumnName("business_unit_id");
            this.Property(t => t.datasource_id).HasColumnName("datasource_id");
            this.Property(t => t.insert_date).HasColumnName("insert_date");
            this.Property(t => t.update_date).HasColumnName("update_date");
            this.Property(t => t.datasource_update_date).HasColumnName("datasource_update_date");
        }
    }
}
