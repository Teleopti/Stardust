using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic
{
	public interface IPayrollResultFactory
	{
		Guid RunPayrollOnBus(PayrollExportDto payrollExport);
	}
}