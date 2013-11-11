using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonDayOffConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = new ScenarioRepository(uow).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var dayOff = new DayOffTemplateRepository(uow).LoadAll().Single(dayOffTemplate => dayOffTemplate.Description.Name.Equals(Name));

			var personDayOff = new PersonAssignment(user, scenario, new DateOnly(Date));
			personDayOff.SetDayOff(dayOff);

			var repository = new PersonAssignmentRepository(uow);

			personDayOff.ScheduleChanged();

			repository.Add(personDayOff);
		}
	}
}