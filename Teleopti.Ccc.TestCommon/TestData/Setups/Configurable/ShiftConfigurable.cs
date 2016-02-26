using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ShiftConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }

		public string ShiftCategory { get; set; }

		private readonly IList<activityInfo> _activities = new List<activityInfo>();

		private class activityInfo
		{
			public string Activity { get; set; }
			public bool IsPersonal { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
		}

		private activityInfo getActivity(int no)
		{
			while (no >= _activities.Count)
				_activities.Add(new activityInfo());
			return _activities.ElementAt(no);
		}

		public string Activity { get { return getActivity(0).Activity; } set { getActivity(0).Activity = value; } }
		public DateTime StartTime { get { return getActivity(0).StartTime; } set { getActivity(0).StartTime = value; } }
		public DateTime EndTime { get { return getActivity(0).EndTime; } set { getActivity(0).EndTime = value; } }
		
		// for legacy scenario texts
		public string ScheduledActivity { get { return getActivity(1).Activity; } set { getActivity(1).Activity = value; } }
		public bool ScheduledActivityIsPersonal { get { return getActivity(1).IsPersonal; } set { getActivity(1).IsPersonal = value; } }
		public DateTime ScheduledActivityStartTime { get { return getActivity(1).StartTime; } set { getActivity(1).StartTime = value; } }
		public DateTime ScheduledActivityEndTime { get { return getActivity(1).EndTime; } set { getActivity(1).EndTime = value; } }

		// for legacy scenario texts
		public string LunchActivity { get { return getActivity(1).Activity; } set { getActivity(1).Activity = value; } }
		public DateTime LunchStartTime { get { return getActivity(1).StartTime; } set { getActivity(1).StartTime = value; } }
		public DateTime LunchEndTime { get { return getActivity(1).EndTime; } set { getActivity(1).EndTime = value; } }

		// for legacy scenario texts
		public string NextActivity { get { return getActivity(1).Activity; } set { getActivity(1).Activity = value; } }
		public DateTime NextActivityStartTime { get { return getActivity(1).StartTime; } set { getActivity(1).StartTime = value; } }
		public DateTime NextActivityEndTime { get { return getActivity(1).EndTime; } set { getActivity(1).EndTime = value; } }

		// this should not be here. this exists on the ShiftCategoryConfigurable
		public string ShiftColor { get; set; }

		public ShiftConfigurable AddActivity(string activity, DateTime startTime, DateTime endTime)
		{
			var activityInfo = getActivity(_activities.Count);
			activityInfo.Activity = activity;
			activityInfo.StartTime = startTime;
			activityInfo.EndTime = endTime;
			return this;
		}


		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			IShiftCategory shiftCategory = null;
			if (ShiftCategory != null)
			{
				shiftCategory = new ShiftCategoryRepository(currentUnitOfWork).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
				if (ShiftColor != null && shiftCategory != null)
					shiftCategory.DisplayColor = Color.FromName(ShiftColor);
			}

			var assignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
			var scenario = new ScenarioRepository(currentUnitOfWork).LoadAll().Single(x => x.Description.Name == Scenario);
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(user, scenario, new DateOnly(StartTime));
			personAssignment.SetShiftCategory(shiftCategory);

			_activities.ForEach(a =>
			{
				var timeZone = user.PermissionInformation.DefaultTimeZone();
				var startTimeUtc = timeZone.SafeConvertTimeToUtc(a.StartTime);
				var endTimeUtc = timeZone.SafeConvertTimeToUtc(a.EndTime);
				var period = new DateTimePeriod(startTimeUtc, endTimeUtc);
				IActivity activity;
				if (a.Activity != null)
				{
					activity = new ActivityRepository(currentUnitOfWork).LoadAll().Single(sCat => sCat.Description.Name.Equals(a.Activity));
				}
				else
				{
					activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
					var activityRepository = new ActivityRepository(currentUnitOfWork);
					activityRepository.Add(activity);
				}
				if (a.IsPersonal)
				{
					personAssignment.AddPersonalActivity(activity, period);
				}
				else
				{
					personAssignment.AddActivity(activity, period);
				}
			});

			assignmentRepository.Add(personAssignment);
		}
		
	}
}
