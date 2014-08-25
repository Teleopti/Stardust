using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ShiftConfigurable : IUserDataSetup
	{
		private DateTimePeriod _assignmentPeriod;

		public string Scenario { get; set; }

		public string ShiftCategory { get; set; }

		public string Activity { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public string ScheduledActivity { get; set; }
		public bool ScheduledActivityIsPersonal { get; set; }
		public DateTime ScheduledActivityStartTime { get; set; }
		public DateTime ScheduledActivityEndTime { get; set; }

		// alternative field name prefix "Lunch"
		public string LunchActivity { get { return ScheduledActivity; } set { ScheduledActivity = value; } }
		public DateTime LunchStartTime { get { return ScheduledActivityStartTime; } set { ScheduledActivityStartTime = value; } }
		public DateTime LunchEndTime { get { return ScheduledActivityEndTime; } set { ScheduledActivityEndTime = value; } }
		
		public string NextActivity { get { return ScheduledActivity; } set { ScheduledActivity = value; } }
		public DateTime NextActivityStartTime { get { return ScheduledActivityStartTime; } set { ScheduledActivityStartTime = value; } }
		public DateTime NextActivityEndTime { get { return ScheduledActivityEndTime; } set { ScheduledActivityEndTime = value; } }
		
		// this should not be here. this exists on the ShiftCategoryConfigurable
		public string ShiftColor { get; set; }	

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			IShiftCategory shiftCategory = null;
			if (ShiftCategory != null)
			{
				shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
				if (ShiftColor != null && shiftCategory != null)
					shiftCategory.DisplayColor = Color.FromName(ShiftColor);
			}

			IActivity activity;
			if (Activity != null)
			{
				activity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(Activity));
			}
			else
			{
				activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
				var activityRepository = new ActivityRepository(uow);
				activityRepository.Add(activity);
			}

			var assignmentRepository = new PersonAssignmentRepository(uow);

			var timeZone = user.PermissionInformation.DefaultTimeZone();
			var startTimeUtc = timeZone.SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = timeZone.SafeConvertTimeToUtc(EndTime);

			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var scenario = new ScenarioRepository(uow).LoadAll().Single(x => x.Description.Name == Scenario);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(user, scenario, new DateOnly(StartTime));
			personAssignment.SetShiftCategory(shiftCategory);
			personAssignment.AddActivity(activity, _assignmentPeriod);
			addScheduleActivity(timeZone, personAssignment, uow);

			// simply publish the schedule changed event so that the read model is updated
			personAssignment.ScheduleChanged();

			assignmentRepository.Add(personAssignment);
		}

		private void addScheduleActivity(TimeZoneInfo timeZone, IPersonAssignment personAssignment, IUnitOfWork uow)
		{
			if (ScheduledActivity == null)
				return;

			var scheduledActivity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ScheduledActivity));

			var startTimeUtc = timeZone.SafeConvertTimeToUtc(ScheduledActivityStartTime);
			var endTimeUtc = timeZone.SafeConvertTimeToUtc(ScheduledActivityEndTime);
			var period = new DateTimePeriod(startTimeUtc, endTimeUtc);

			if (ScheduledActivityIsPersonal)
				personAssignment.AddPersonalActivity(scheduledActivity, period);
			else
				personAssignment.AddActivity(scheduledActivity, period);
		}
	}
}