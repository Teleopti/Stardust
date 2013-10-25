using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonDayOffConfigurable : IUserDataSetup
	{
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var dayOff = new DayOffTemplateRepository(uow).LoadAll().Single(dayOffTemplate => dayOffTemplate.Description.Name.Equals(Name));
			var personDayOff = new PersonAssignment(user, Scenario, new DateOnly(Date));
			personDayOff.SetDayOff(dayOff);

			var repository = new PersonAssignmentRepository(uow);

			personDayOff.ScheduleChanged();

			repository.Add(personDayOff);
		}
	}
}
