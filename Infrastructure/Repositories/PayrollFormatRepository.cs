using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPayrollFormatRepository : IRepository<IPayrollFormat>
	{
	}
	public class PayrollFormatRepository : Repository<IPayrollFormat>, IPayrollFormatRepository
	{
		public PayrollFormatRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}


		public PayrollFormatRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public override bool ValidateUserLoggedOn
		{
			get
			{
				return false;
			}
		}
	}
}
