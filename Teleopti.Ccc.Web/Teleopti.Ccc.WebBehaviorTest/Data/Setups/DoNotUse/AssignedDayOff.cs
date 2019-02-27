using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


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

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			if (DayOff == null)
			{
				DayOff = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var repository = new DayOffTemplateRepository(unitOfWork);
				repository.Add(DayOff);
			}

			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			var ass = new PersonAssignment(person, Scenario, new DateOnly(DateTime.Parse(Date)));
			ass.SetDayOff(DayOff);
			personAssignmentRepository.Add(ass);
		}
	}
}