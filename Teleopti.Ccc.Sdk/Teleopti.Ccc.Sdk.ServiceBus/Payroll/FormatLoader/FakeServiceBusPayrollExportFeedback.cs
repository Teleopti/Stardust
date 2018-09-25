using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	public class FakeServiceBusPayrollExportFeedback : PayrollExportFeedbackEx
	{
		private void internalAddPayrollDetail(DetailLevel detailLevel, string message, Exception exception = null)
		{
			PayrollResultDetails.Add(new PayrollResultDetailData(detailLevel, message, exception, DateTime.UtcNow));
		}
		public override void ReportProgress(int percentage, string information)
		{
			internalAddPayrollDetail(DetailLevel.Info, $"{information} + {percentage}%");
		}

		public FakeServiceBusPayrollExportFeedback(InterAppDomainArguments interAppDomainArguments) : base(interAppDomainArguments)
		{
		}
	}
}