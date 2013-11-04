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

		public string PersonalActivity { get; set; }
		public DateTime PersonalActivityStartTime { get; set; }
		public DateTime PersonalActivityEndTime { get; set; }

		public string Overtime { get; set; }
		public string OvertimeMultiplicatorDefinitionSet { get; set; }
		public DateTime OvertimeStartTime { get; set; }
		public DateTime OvertimeEndTime { get; set; }

		public string LunchActivity { get; set; }
		public bool Lunch3HoursAfterStart { get; set; }
		public DateTime LunchStartTime { get; set; }
		public DateTime LunchEndTime { get; set; }

		public string ShiftColor { get; set; }	// this should not be here. this exists on the ShiftCategoryConfigurable

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
			if (ShiftColor != null)
				shiftCategory.DisplayColor = Color.FromName(ShiftColor);

			var activity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(Activity));
			var lunchActivity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(LunchActivity));

			var assignmentRepository = new PersonAssignmentRepository(uow);

			var timeZone = user.PermissionInformation.DefaultTimeZone();
			var startTimeUtc = timeZone.SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = timeZone.SafeConvertTimeToUtc(EndTime);

			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var scenario = new ScenarioRepository(uow).LoadAll().Single(x => x.Description.Name == Scenario);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, scenario, new DateOnly(StartTime));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, _assignmentPeriod, shiftCategory);

			// add lunch
			DateTimePeriod? lunchPeriod = null;
			if (LunchStartTime != DateTime.MinValue)
			{
				var lunchStartTimeUtc = timeZone.SafeConvertTimeToUtc(LunchStartTime);
				var lunchEndTimeUtc = timeZone.SafeConvertTimeToUtc(LunchEndTime);
				lunchPeriod = new DateTimePeriod(lunchStartTimeUtc, lunchEndTimeUtc);
			}
			else if (Lunch3HoursAfterStart)
			{
				lunchPeriod = new DateTimePeriod(startTimeUtc.AddHours(3), startTimeUtc.AddHours(4));
			}
			if (lunchPeriod.HasValue)
				mainShift.LayerCollection.Add(new EditableShiftLayer(lunchActivity, lunchPeriod.Value));

			new EditableShiftMapper().SetMainShiftLayers(assignment, mainShift);

			if (PersonalActivity != null)
			{
				var personalActivity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(PersonalActivity));
				var personalActivityStartTimeUtc = timeZone.SafeConvertTimeToUtc(PersonalActivityStartTime);
				var personalActivityEndTimeUtc = timeZone.SafeConvertTimeToUtc(PersonalActivityEndTime);
				var personalActivityPeriod = new DateTimePeriod(personalActivityStartTimeUtc, personalActivityEndTimeUtc);
				assignment.AddPersonalLayer(personalActivity, personalActivityPeriod);
			}

			if (Overtime != null)
			{
				var multiplicatorDefinitionSet = new MultiplicatorDefinitionSetRepository(uow).LoadAll().Single(x => x.Name.Equals(OvertimeMultiplicatorDefinitionSet));
				var overtimeStartTimeUtc = timeZone.SafeConvertTimeToUtc(OvertimeStartTime);
				var overtimeEndTimeUtc = timeZone.SafeConvertTimeToUtc(OvertimeEndTime);
				var overtimePeriod = new DateTimePeriod(overtimeStartTimeUtc, overtimeEndTimeUtc);
				assignment.AddOvertimeLayer(activity, overtimePeriod, multiplicatorDefinitionSet);
			}

			// simply publish the schedule changed event so that the read model is updated
			assignment.ScheduleChanged();

			assignmentRepository.Add(assignment);
		}
	}
}