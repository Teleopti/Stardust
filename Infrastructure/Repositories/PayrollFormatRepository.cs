using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPayrollFormatRepository : IRepository<IPayrollFormat>
	{
	}

	public class PayrollFormatRepository : Repository<IPayrollFormat>, IPayrollFormatRepository
	{
		public PayrollFormatRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{

		}
	}
}
