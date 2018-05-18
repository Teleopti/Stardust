using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IInitializePayrollFormats
	{
		void Initialize();
		void InitializeOneTenant(IList<PayrollFormatDto> tenantPayrollFormats, string tenantName);

	}
}