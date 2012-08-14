using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class CommonContractSchedule : IDataSetup, IContractScheduleSetup
	{
		public IContractSchedule ContractSchedule { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			ContractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			ContractSchedule.Description = new Description("Contract schedule with 2 days off");
			new ContractScheduleRepository(uow).Add(ContractSchedule);
		}
	}
}