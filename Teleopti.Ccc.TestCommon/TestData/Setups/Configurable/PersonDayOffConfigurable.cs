using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonDayOffConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = ScenarioRepository.DONT_USE_CTOR(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var dayOff = DayOffTemplateRepository.DONT_USE_CTOR2(unitOfWork).LoadAll().Single(dayOffTemplate => dayOffTemplate.Description.Name.Equals(Name));

			var personDayOff = new PersonAssignment(person, scenario, new DateOnly(Date));
			personDayOff.SetDayOff(dayOff);

			var repository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			
			repository.Add(personDayOff);
		}
	}
}