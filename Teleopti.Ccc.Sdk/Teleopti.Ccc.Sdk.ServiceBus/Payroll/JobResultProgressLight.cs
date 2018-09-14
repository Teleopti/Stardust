using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	[Serializable]
	public class JobResultProgressLight : IJobResultProgress
	{
		public JobResultProgressLight(int totalPercentage = 100)
		{
			TotalPercentage = totalPercentage;
		}

		public int Percentage { get; set; }
		public string Message { get; set; }
		public Guid JobResultId { get; set; }
		public int TotalPercentage { get; set; }
	}
}