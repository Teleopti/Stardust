using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class AssignedDayOff : IUserDataSetup
	{
		private static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);

		public string Date { get; set; }
		public IDayOffTemplate DayOff;
		public readonly IScenario Scenario = DefaultScenario.Scenario;

		public AssignedDayOff()
		{
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			if (DayOff == null)
			{
				DayOff = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var repository = new DayOffTemplateRepository(currentUnitOfWork);
				repository.Add(DayOff);
			}

			var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
			var ass = new PersonAssignment(user, Scenario, new DateOnly(DateTime.Parse(Date)));
			ass.SetDayOff(DayOff);
			personAssignmentRepository.Add(ass);
		}
	}
}