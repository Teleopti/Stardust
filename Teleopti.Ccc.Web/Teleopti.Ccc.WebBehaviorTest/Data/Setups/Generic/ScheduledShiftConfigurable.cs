using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ScheduledShiftConfigurable : IUserDataSetup
	{
		private DateTimePeriod _assignmentPeriod;

		public string ShiftCategory { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var shiftCat = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));

			var assignmentRepository = new PersonAssignmentRepository(uow);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario);
			assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, _assignmentPeriod, shiftCat));

			assignmentRepository.Add(assignment);
		}

	}
}