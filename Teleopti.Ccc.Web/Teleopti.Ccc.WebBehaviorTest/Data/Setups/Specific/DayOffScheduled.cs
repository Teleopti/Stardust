using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class DayOffScheduled : IUserDataSetup
	{

		private readonly int _weekday;
		public readonly IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		public readonly IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public DayOffScheduled(int weekday)
		{
			_weekday = weekday;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = new DateOnly(DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo).AddDays(_weekday - 1));
			var ass = new PersonAssignment(user, Scenario, date);
			ass.SetDayOff(DayOffTemplate);
			var personAssignmentRepository = new PersonAssignmentRepository(uow);
			personAssignmentRepository.Add(ass);
		}
	}
}