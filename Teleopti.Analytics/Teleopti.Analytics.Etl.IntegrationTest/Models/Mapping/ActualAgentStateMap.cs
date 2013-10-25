using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class ActualAgentStateMap : EntityTypeConfiguration<ActualAgentState>
    {
        public ActualAgentStateMap()
        {
            // Primary Key
            this.HasKey(t => t.PersonId);

            // Properties
            this.Property(t => t.StateCode)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.State)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Scheduled)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ScheduledNext)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.AlarmName)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.OriginalDataSourceId)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("ActualAgentState", "RTA");
            this.Property(t => t.PersonId).HasColumnName("PersonId");
            this.Property(t => t.StateCode).HasColumnName("StateCode");
            this.Property(t => t.PlatformTypeId).HasColumnName("PlatformTypeId");
            this.Property(t => t.State).HasColumnName("State");
            this.Property(t => t.StateId).HasColumnName("StateId");
            this.Property(t => t.Scheduled).HasColumnName("Scheduled");
            this.Property(t => t.ScheduledId).HasColumnName("ScheduledId");
            this.Property(t => t.StateStart).HasColumnName("StateStart");
            this.Property(t => t.ScheduledNext).HasColumnName("ScheduledNext");
            this.Property(t => t.ScheduledNextId).HasColumnName("ScheduledNextId");
            this.Property(t => t.NextStart).HasColumnName("NextStart");
            this.Property(t => t.AlarmName).HasColumnName("AlarmName");
            this.Property(t => t.AlarmId).HasColumnName("AlarmId");
            this.Property(t => t.Color).HasColumnName("Color");
            this.Property(t => t.AlarmStart).HasColumnName("AlarmStart");
            this.Property(t => t.StaffingEffect).HasColumnName("StaffingEffect");
            this.Property(t => t.ReceivedTime).HasColumnName("ReceivedTime");
            this.Property(t => t.BatchId).HasColumnName("BatchId");
            this.Property(t => t.OriginalDataSourceId).HasColumnName("OriginalDataSourceId");
        }
    }
}
