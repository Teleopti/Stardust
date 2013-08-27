using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class DayOffToday : IUserDataSetup
	{
		public DateOnly Date = DateOnly.Today;
		public readonly IDayOffTemplate DayOff = TestData.DayOffTemplate;
		public readonly IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var personAssignmentRepository = new PersonAssignmentRepository(uow);
			var ass = new PersonAssignment(user, Scenario, Date);
			ass.SetDayOff(DayOff);
			personAssignmentRepository.Add(ass);
		}
	}
}