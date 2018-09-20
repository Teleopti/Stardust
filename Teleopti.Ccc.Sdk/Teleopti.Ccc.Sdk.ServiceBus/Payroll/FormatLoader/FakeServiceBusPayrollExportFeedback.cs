using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	public class FakeServiceBusPayrollExportFeedback : IServiceBusPayrollExportFeedback
	{
		public List<IPayrollResultDetail> ProgressList = new List<IPayrollResultDetail>();

		public void ReportProgress(int percentage, string information)
		{
			ProgressList.Add(new PayrollResultDetail(DetailLevel.Info, information, DateTime.UtcNow, null));
		}

		public void Error(string message)
		{
			//throw new NotImplementedException();
		}

		public void Error(string message, Exception exception)
		{
			ProgressList.Add(new PayrollResultDetail(DetailLevel.Error, message, DateTime.UtcNow, exception));
		}

		public void Warning(string message)
		{
			//throw new NotImplementedException();
		}

		public void Warning(string message, Exception exception)
		{
			//throw new NotImplementedException();
		}

		public void Info(string message)
		{
			//throw new NotImplementedException();
		}

		public void Info(string message, Exception exception)
		{
			//throw new NotImplementedException();
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		public void SetPayrollResult(IPayrollResult payrollResult)
		{
			//throw new NotImplementedException();
		}

		public void AddPayrollResultDetail(IPayrollResultDetail payrollResultDetail)
		{
			ProgressList.Add(payrollResultDetail);
		}
	}
}