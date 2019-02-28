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

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			if (DayOffTemplate == null)
			{
				DayOffTemplate = DayOffFactory.CreateDayOff(new Description(RandomName.Make(), RandomName.Make()));
				var repository = DayOffTemplateRepository.DONT_USE_CTOR2(unitOfWork);
				repository.Add(DayOffTemplate);
			}

			var ass = new PersonAssignment(person, Scenario, new DateOnly(_date));
			ass.SetDayOff(DayOffTemplate);
			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			personAssignmentRepository.Add(ass);
		}
	}
}