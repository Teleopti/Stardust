using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonalShiftConfigurable : IUserDataSetup
	{
		public string Activity { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var shiftCat = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals("Day"));
			var activity = new ActivityRepository(uow).LoadAll().Single(a => a.Name == Activity);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(EndTime);

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				activity,
				user,
				new DateTimePeriod(startTimeUtc, endTimeUtc),
				shiftCat,
				scenario);
			var repository = new PersonAssignmentRepository(uow);
			repository.Add(assignment);
		}
	}
}