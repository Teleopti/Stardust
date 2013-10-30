using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class DBA_VirtualFileStatsHistory
    {
        public int RecordID { get; set; }
        public int database_id { get; set; }
        public int file_id { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string PhysicalName { get; set; }
        public Nullable<long> num_of_reads { get; set; }
        public Nullable<long> num_of_reads_from_start { get; set; }
        public Nullable<long> num_of_writes { get; set; }
        public Nullable<long> num_of_writes_from_start { get; set; }
        public Nullable<long> num_of_bytes_read { get; set; }
        public Nullable<long> num_of_bytes_read_from_start { get; set; }
        public Nullable<long> num_of_bytes_written { get; set; }
        public Nullable<long> num_of_bytes_written_from_start { get; set; }
        public Nullable<long> io_stall { get; set; }
        public Nullable<long> io_stall_from_start { get; set; }
        public Nullable<long> io_stall_read_ms { get; set; }
        public Nullable<long> io_stall_read_ms_from_start { get; set; }
        public Nullable<long> io_stall_write_ms { get; set; }
        public Nullable<long> io_stall_write_ms_from_start { get; set; }
        public Nullable<System.DateTime> RecordedDateTime { get; set; }
        public Nullable<long> interval_ms { get; set; }
        public Nullable<bool> FirstMeasureFromStart { get; set; }
    }
}
