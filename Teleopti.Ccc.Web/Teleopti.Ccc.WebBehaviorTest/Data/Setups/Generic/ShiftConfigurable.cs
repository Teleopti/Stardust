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
	public class ShiftConfigurable : IUserDataSetup
	{
		private DateTimePeriod _assignmentPeriod;

		public string ShiftCategoryName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool Lunch3HoursAfterStart { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			//för nu sparar ett shiftcategory hårt
			//var shiftCat =new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategoryName));
			var shiftCat = new ShiftCategory("will be removed");
			new ShiftCategoryRepository(uow).Add(shiftCat);

			var endDateUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime.Date);
			var assignmentRepository = new PersonAssignmentRepository(uow);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario);
			assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, _assignmentPeriod, shiftCat));

			// add lunch
			if (Lunch3HoursAfterStart)
			{
				var lunchPeriod = new DateTimePeriod(startTimeUtc.AddHours(3), startTimeUtc.AddHours(4));
				assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, lunchPeriod));
			}

			assignmentRepository.Add(assignment);
		}

		public TimeSpan GetContractTime()
		{
			// rolling my own contract time calculation.
			// do we need to do a projection here really?
			var contractTime = _assignmentPeriod.ElapsedTime();
			if (Lunch3HoursAfterStart)
				contractTime = contractTime.Subtract(TimeSpan.FromHours(1));
			return contractTime;
		}

	}
}