using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class ExternalAgentStateMap : EntityTypeConfiguration<ExternalAgentState>
    {
        public ExternalAgentStateMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Id, t.LogOn, t.StateCode, t.TimeInState, t.TimestampValue, t.IsSnapshot });

            // Properties
            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.LogOn)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.StateCode)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.TimeInState)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("ExternalAgentState", "RTA");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.LogOn).HasColumnName("LogOn");
            this.Property(t => t.StateCode).HasColumnName("StateCode");
            this.Property(t => t.TimeInState).HasColumnName("TimeInState");
            this.Property(t => t.TimestampValue).HasColumnName("TimestampValue");
            this.Property(t => t.PlatformTypeId).HasColumnName("PlatformTypeId");
            this.Property(t => t.DataSourceId).HasColumnName("DataSourceId");
            this.Property(t => t.BatchId).HasColumnName("BatchId");
            this.Property(t => t.IsSnapshot).HasColumnName("IsSnapshot");
        }
    }
}
