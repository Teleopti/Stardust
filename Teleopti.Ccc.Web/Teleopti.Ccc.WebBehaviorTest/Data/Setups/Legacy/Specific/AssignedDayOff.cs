using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AssignedDayOff : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);

		public string Date { get; set; }
		public readonly IDayOffTemplate DayOff = TestData.DayOffTemplate;
		public readonly IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public AssignedDayOff()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var personAssignmentRepository = new PersonAssignmentRepository(uow);
			var ass = new PersonAssignment(user, Scenario, Date);
			ass.SetDayOff(DayOff);
			personAssignmentRepository.Add(ass);
		}
	}
}