﻿using System;
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

		public string LunchActivity { get; set; }
		public bool Lunch3HoursAfterStart { get; set; }
		public DateTime LunchStartTime { get; set; }
		public DateTime LunchEndTime { get; set; }

		public string ShiftColor { get; set; }	// this should not be here. this exists on the ShiftCategoryConfigurable
		public string AllActivityColor { get; set; }// this should not be here. this should exist on the ActivityConfigurable

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
			if (ShiftColor != null)
				shiftCategory.DisplayColor = Color.FromName(ShiftColor);

			IActivity activity;
			if (AllActivityColor != null)
			{
				activity = new Activity("Phone");
				new ActivityRepository(uow).Add(activity);
				activity.DisplayColor = Color.FromName(AllActivityColor);
			}
			else
			{
				activity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(Activity));
			}

			IActivity lunchActivity;
			if (AllActivityColor != null)
			{
				lunchActivity = new Activity("Lunch");
				new ActivityRepository(uow).Add(lunchActivity);
				lunchActivity.DisplayColor = Color.FromName(AllActivityColor);
			}
			else
			{
				lunchActivity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(LunchActivity));
			}



			var assignmentRepository = new PersonAssignmentRepository(uow);

			var startTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(EndTime);

			_assignmentPeriod = new DateTimePeriod(startTimeUtc, endTimeUtc);
			var scenario = new ScenarioRepository(uow).LoadAll().Single(x => x.Description.Name == Scenario);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, scenario, new DateOnly(StartTime));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, _assignmentPeriod, shiftCategory);

			// add lunch
			DateTimePeriod? lunchPeriod = null;
			if (LunchStartTime != DateTime.MinValue)
			{
				var lunchStartTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(LunchStartTime);
				var lunchEndTimeUtc = user.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(LunchEndTime);
				lunchPeriod = new DateTimePeriod(lunchStartTimeUtc, lunchEndTimeUtc);
			}
			else if (Lunch3HoursAfterStart)
			{
				lunchPeriod = new DateTimePeriod(startTimeUtc.AddHours(3), startTimeUtc.AddHours(4));
			}
			if (lunchPeriod.HasValue)
				mainShift.LayerCollection.Add(new EditableShiftLayer(lunchActivity, lunchPeriod.Value));

			new EditableShiftMapper().SetMainShiftLayers(assignment, mainShift);


			// simply publish the schedule changed event so that the read model is updated
			assignment.ScheduleChanged();

			assignmentRepository.Add(assignment);
		}
	}
}