using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAbsenceConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = ScenarioRepository.DONT_USE_CTOR(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var absence = new AbsenceRepository(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Name));

			var personAbsence = new PersonAbsence(scenario);
			personAbsence.AddExplicitAbsence(person, absence, StartTime, EndTime);

			var repository = new PersonAbsenceRepository(unitOfWork);
			repository.Add(personAbsence);
		}
	}
}