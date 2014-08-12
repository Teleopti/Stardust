using System;
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
	public class DayOffScheduled : IUserDataSetup
	{
		private readonly DateTime _date;

		public readonly IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;
		public readonly IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public DayOffScheduled(DateTime date)
		{
			_date = date;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var ass = new PersonAssignment(user, Scenario, new DateOnly(_date));
			ass.SetDayOff(DayOffTemplate);
			var personAssignmentRepository = new PersonAssignmentRepository(uow);
			personAssignmentRepository.Add(ass);
		}
	}
}