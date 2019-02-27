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
	public class PersonEmptyDayConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public DateTime Date { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = ScenarioRepository.DONT_USE_CTOR(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var emptyPersonAssignment = new PersonAssignment(person, scenario, new DateOnly(Date));
			emptyPersonAssignment.Clear();

			var repository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			
			repository.Add(emptyPersonAssignment);
		}
	}
}