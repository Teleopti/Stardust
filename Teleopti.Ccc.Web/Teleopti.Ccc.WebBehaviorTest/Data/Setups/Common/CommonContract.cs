using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{
	public class CommonContract : IDataSetup, IContractSetup
	{
		public IContract Contract { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract(RandomName.Make("Common contract"));
			new ContractRepository(uow).Add(Contract);
		}
	}
}