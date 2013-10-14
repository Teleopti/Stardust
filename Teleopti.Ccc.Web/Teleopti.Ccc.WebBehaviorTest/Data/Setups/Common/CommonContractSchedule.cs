using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{
	public class CommonContractSchedule : IDataSetup, IContractScheduleSetup
	{
		public IContractSchedule ContractSchedule { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			ContractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			ContractSchedule.Description = new Description(DefaultName.Make("Common contract schedule"));
			new ContractScheduleRepository(uow).Add(ContractSchedule);
		}
	}
}