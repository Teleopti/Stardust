using System;
using System.Drawing;
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
		public string ShiftCategory { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool Lunch3HoursAfterStart { get; set; }
		public string ShiftColor { get; set; }
		public string AllActivityColor { get; set; }
		public string Activity { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var shiftCat = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));

			if (ShiftColor != null)
				shiftCat.DisplayColor = Color.FromName(ShiftColor);


			var assignmentRepository = new PersonAssignmentRepository(uow);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario);

			if (AllActivityColor != null)
			{
				var activity = new Activity("Phone");
				new ActivityRepository(uow).Add(activity);
				activity.DisplayColor = Color.FromName(AllActivityColor);
				assignment.SetMainShift(MainShiftFactory.CreateMainShift(activity, _assignmentPeriod, shiftCat));
			}
			else
			{
				if (Activity == null)
				{
					var activityPhone = TestData.ActivityPhone;
					assignment.SetMainShift(MainShiftFactory.CreateMainShift(activityPhone, _assignmentPeriod, shiftCat));
				}
			}

			if (Activity != null)
			{
				var activityRepository = new ActivityRepository(uow);
				var activities = activityRepository.LoadAll();
				var mainActivity = activities.Single(a => a.Name == Activity);
				assignment.SetMainShift(MainShiftFactory.CreateMainShift(mainActivity, _assignmentPeriod, shiftCat));
			}



			// add lunch
			if (Lunch3HoursAfterStart)
			{
				var lunchPeriod = new DateTimePeriod(startTimeUtc.AddHours(3), startTimeUtc.AddHours(4));

				if (AllActivityColor != null)
				{
					var lunchActivity = new Activity("Lunch");
					new ActivityRepository(uow).Add(lunchActivity);
					lunchActivity.DisplayColor = Color.FromName(AllActivityColor);
					assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(lunchActivity, lunchPeriod));
				}
				else
				{
					assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, lunchPeriod));
				}
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