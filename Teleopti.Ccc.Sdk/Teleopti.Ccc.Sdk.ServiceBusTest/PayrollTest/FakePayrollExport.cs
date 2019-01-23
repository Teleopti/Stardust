using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	public class FakePayrollExport : PayrollExport
	{
		public void SetCreatedBy(IPerson person)
		{
			CreatedBy = person;
		}
	}
}