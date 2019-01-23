using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	public class FakePayrollPeopleLoader : IPayrollPeopleLoader
	{
		public IPerson GetOwningPerson(RunPayrollExportEvent message, IUnitOfWork unitOfWork)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> GetPeopleForExport(RunPayrollExportEvent message, DateOnlyPeriod payrollExportPeriod,
			IUnitOfWork unitOfWork)
		{
			return new List<IPerson>();
		}
	}
}