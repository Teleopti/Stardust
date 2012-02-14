using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PayrollResultDto : Dto
    {
    	public PayrollResultDto()
    	{
    		Details = new List<PayrollResultDetailDto>();
    	}

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public bool FinishedOk { get; set; }

        [DataMember]
        public bool IsWorking { get; set; }

        [DataMember]
        public ICollection<PayrollResultDetailDto> Details { get; private set; }
    }
}