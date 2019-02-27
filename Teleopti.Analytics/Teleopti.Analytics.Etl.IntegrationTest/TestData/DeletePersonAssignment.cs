using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public class DeletePersonAssignment : IUserDataSetup
	{
		private readonly IScenario _scenario;
		private readonly DateOnly _theDateOnly;

		public DeletePersonAssignment(IScenario scenario, DateOnly theDateOnly)
		{
			_scenario = scenario;
			_theDateOnly = theDateOnly;
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var assignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			var assignment = assignmentRepository.Find(new List<IPerson> {person}, new DateOnlyPeriod(_theDateOnly, _theDateOnly), _scenario).First();
			
			assignmentRepository.Remove(assignment);
		}


	}
}