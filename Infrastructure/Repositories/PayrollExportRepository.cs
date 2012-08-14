using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PayrollExportRepository : Repository<IPayrollExport>, IPayrollExportRepository
    {
        public PayrollExportRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public PayrollExportRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}
