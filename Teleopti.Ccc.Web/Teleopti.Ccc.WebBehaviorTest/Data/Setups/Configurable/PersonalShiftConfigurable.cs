using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonalShiftConfigurable : IUserDataSetup
	{
		public string Activity { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;

			var activity = new ActivityRepository(currentUnitOfWork).LoadAll().Single(a => a.Name == Activity);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(
				activity,
				user,
				new DateTimePeriod(startTimeUtc, endTimeUtc),
				scenario);
			var repository = new PersonAssignmentRepository(currentUnitOfWork);
			repository.Add(assignment);
		}
	}
}