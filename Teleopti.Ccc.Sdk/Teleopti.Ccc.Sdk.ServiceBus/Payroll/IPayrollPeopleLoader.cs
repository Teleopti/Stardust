using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPayrollPeopleLoader
    {
        IPerson GetOwningPerson(RunPayrollExportEvent message, IUnitOfWork unitOfWork);
        IEnumerable<IPerson> GetPeopleForExport(RunPayrollExportEvent message, DateOnlyPeriod payrollExportPeriod, IUnitOfWork unitOfWork);
    }
}