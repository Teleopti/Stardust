using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common
{
	public class CommonContract : IDataSetup
	{
		public IContract Contract { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			Contract = new Contract(DefaultName.Make("Common contract"));
			new ContractRepository(uow).Add(Contract);
		}
	}
}