using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class DayOffScheduled : IUserDataSetup
	{
		private readonly DateTime _date;
		public IDayOffTemplate DayOffTemplate;
		public readonly IScenario Scenario = DefaultScenario.Scenario;

		public DayOffScheduled(DateTime date)
		{
			_date = date;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			if (DayOffTemplate == null)
			{
				DayOffTemplate = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var repository = new DayOffTemplateRepository(currentUnitOfWork);
				repository.Add(DayOffTemplate);
			}

			var ass = new PersonAssignment(user, Scenario, new DateOnly(_date));
			ass.SetDayOff(DayOffTemplate);
			var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
			personAssignmentRepository.Add(ass);
		}
	}
}