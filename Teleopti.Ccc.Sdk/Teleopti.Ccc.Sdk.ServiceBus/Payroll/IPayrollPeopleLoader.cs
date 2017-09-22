using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPayrollPeopleLoader
    {
        IPerson GetOwningPerson(RunPayrollExportEvent message, IUnitOfWork unitOfWork);
        IEnumerable<IPerson> GetPeopleForExport(RunPayrollExportEvent message, DateOnlyPeriod payrollExportPeriod, IUnitOfWork unitOfWork);
    }
}