using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [DataContract]
    public class ScheduleTagDto: Dto
    {
        [DataMember]
        public string Description { get; set; }
    }
}
