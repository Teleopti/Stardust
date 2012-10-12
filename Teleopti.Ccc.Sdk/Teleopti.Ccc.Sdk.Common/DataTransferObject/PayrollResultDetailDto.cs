using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/05/")]
    public class PayrollResultDetailDto : Dto
    {
        [DataMember]
        public DetailLevel DetailLevel { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public string ExceptionStackTrace { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}