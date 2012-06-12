using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class CommonContract : IDataSetup, IContractSetup
	{
		public IContract Contract { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract("Common contract");
			new ContractRepository(uow).Add(Contract);
		}
	}
}