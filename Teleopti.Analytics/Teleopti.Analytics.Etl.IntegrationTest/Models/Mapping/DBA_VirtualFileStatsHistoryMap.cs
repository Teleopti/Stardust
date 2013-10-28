using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class DBA_VirtualFileStatsHistoryMap : EntityTypeConfiguration<DBA_VirtualFileStatsHistory>
    {
        public DBA_VirtualFileStatsHistoryMap()
        {
            // Primary Key
            this.HasKey(t => new { t.RecordID, t.database_id, t.file_id, t.ServerName, t.DatabaseName, t.PhysicalName });

            // Properties
            this.Property(t => t.RecordID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.database_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.file_id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.ServerName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.DatabaseName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.PhysicalName)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("DBA_VirtualFileStatsHistory");
            this.Property(t => t.RecordID).HasColumnName("RecordID");
            this.Property(t => t.database_id).HasColumnName("database_id");
            this.Property(t => t.file_id).HasColumnName("file_id");
            this.Property(t => t.ServerName).HasColumnName("ServerName");
            this.Property(t => t.DatabaseName).HasColumnName("DatabaseName");
            this.Property(t => t.PhysicalName).HasColumnName("PhysicalName");
            this.Property(t => t.num_of_reads).HasColumnName("num_of_reads");
            this.Property(t => t.num_of_reads_from_start).HasColumnName("num_of_reads_from_start");
            this.Property(t => t.num_of_writes).HasColumnName("num_of_writes");
            this.Property(t => t.num_of_writes_from_start).HasColumnName("num_of_writes_from_start");
            this.Property(t => t.num_of_bytes_read).HasColumnName("num_of_bytes_read");
            this.Property(t => t.num_of_bytes_read_from_start).HasColumnName("num_of_bytes_read_from_start");
            this.Property(t => t.num_of_bytes_written).HasColumnName("num_of_bytes_written");
            this.Property(t => t.num_of_bytes_written_from_start).HasColumnName("num_of_bytes_written_from_start");
            this.Property(t => t.io_stall).HasColumnName("io_stall");
            this.Property(t => t.io_stall_from_start).HasColumnName("io_stall_from_start");
            this.Property(t => t.io_stall_read_ms).HasColumnName("io_stall_read_ms");
            this.Property(t => t.io_stall_read_ms_from_start).HasColumnName("io_stall_read_ms_from_start");
            this.Property(t => t.io_stall_write_ms).HasColumnName("io_stall_write_ms");
            this.Property(t => t.io_stall_write_ms_from_start).HasColumnName("io_stall_write_ms_from_start");
            this.Property(t => t.RecordedDateTime).HasColumnName("RecordedDateTime");
            this.Property(t => t.interval_ms).HasColumnName("interval_ms");
            this.Property(t => t.FirstMeasureFromStart).HasColumnName("FirstMeasureFromStart");
        }
    }
}
