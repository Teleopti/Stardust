using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollFormatRepositoryFactory : IPayrollFormatRepositoryFactory
	{
		public IPayrollFormatRepository CreatePayrollFormatRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PayrollFormatRepository(currentUnitOfWork);
		}
	}

	public class FakePayrollFormatRepositoryFactory : IPayrollFormatRepositoryFactory
	{
		public FakePayrollFormatRepository CurrentPayrollFormatRepository;

		public IPayrollFormatRepository CreatePayrollFormatRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			CurrentPayrollFormatRepository = new FakePayrollFormatRepository();
			return CurrentPayrollFormatRepository;
		}
	}

	public interface IPayrollFormatRepositoryFactory
	{
		IPayrollFormatRepository CreatePayrollFormatRepository(ICurrentUnitOfWork currentUnitOfWork);
	}
}
