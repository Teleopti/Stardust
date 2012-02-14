using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPayrollPeopleLoader
    {
        IPerson GetOwningPerson(RunPayrollExport message, IUnitOfWork unitOfWork);
        IEnumerable<IPerson> GetPeopleForExport(RunPayrollExport message, DateTimePeriod payrollExportPeriod, IUnitOfWork unitOfWork);
    }
}