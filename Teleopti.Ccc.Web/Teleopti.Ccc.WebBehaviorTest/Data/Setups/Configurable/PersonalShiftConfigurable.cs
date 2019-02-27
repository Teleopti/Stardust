using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonalShiftConfigurable : IUserDataSetup
	{
		public string Activity { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;

			var activity = new ActivityRepository(unitOfWork, null, null).LoadAll().Single(a => a.Name == Activity);

			var startTimeUtc = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(person,
				scenario, activity, new DateTimePeriod(startTimeUtc, endTimeUtc));
			var repository = PersonAssignmentRepository.DONT_USE_CTOR(unitOfWork);
			repository.Add(assignment);
		}
	}
}