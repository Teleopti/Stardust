using System;
using System.Windows.Media;

namespace Teleopti.Ccc.Sdk.SimpleSample.Model
{
    public class ScheduleDayDetailModel
    {
        public DateTime LayerStart { get; set; }
        public DateTime LayerEnd { get; set; }
        public string PayloadName { get; set; }
        public string PayloadPayrollCode { get; set; }
        public string OvertimeName { get; set; }
        public Color DisplayColor { get; set; }
        public string MeetingSubject { get; set; }
        public string MeetingLocation { get; set; }
        public Guid PayloadId { get; set; }
        public bool InContractTime { get; set; }
        public bool InPaidTime { get; set; }
        public bool InWorkTime { get; set; }

        public Guid? OvertimeDefinitionSetId { get; set; }
        public Guid? MeetingId { get; set; }
    }
}