using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPayrollResultRepository : IRepository<IPayrollResult>
    {
        ICollection<IPayrollResult> GetPayrollResultsByPayrollExport(IPayrollExport payrollExport);
    }
}
