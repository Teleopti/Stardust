using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ContractFromTable : IDataSetup, IContractSetup
	{
		public int PositiveDayOffTolerance { get; set; }
		public int NegativeDayOffTolerance { get; set; }

		public IContract Contract { get; set; }
		
		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract("Contract from table");
			Contract.PositiveDayOffTolerance = PositiveDayOffTolerance;
			Contract.NegativeDayOffTolerance = NegativeDayOffTolerance;
			new ContractRepository(uow).Add(Contract);
		}
	}
}