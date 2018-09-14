using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	[Serializable]
	public class PayrollResultDetailData
	{
		public PayrollResultDetailData(DetailLevel detailLevel, string message, Exception exception, DateTime timeStamp)
		{
			DetailLevel = detailLevel;
			Message = message;
			Exception = exception;
			TimeStamp = timeStamp;
		}

		public string Message { get; }
		public DetailLevel DetailLevel { get; }
		public Exception Exception { get; }
		public DateTime TimeStamp { get; }
	}
}
