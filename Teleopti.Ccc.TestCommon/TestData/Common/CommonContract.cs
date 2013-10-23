using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Common
{
	public class CommonContract : IContractSetup
	{
		public IContract Contract { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract(DefaultName.Make("Common contract"));
			new ContractRepository(uow).Add(Contract);
		}
	}
}