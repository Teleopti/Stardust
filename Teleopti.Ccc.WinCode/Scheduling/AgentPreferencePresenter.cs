using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentPreferencePresenter
	{
		private readonly IAgentPreferenceView _view;
		private readonly IScheduleDay _scheduleDay;

		public AgentPreferencePresenter(IAgentPreferenceView view, IScheduleDay scheduleDay)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		}

		public IAgentPreferenceView View
		{
			get { return _view; }
		}

		public IScheduleDay ScheduleDay
		{
			get { return _scheduleDay; }
		}

		public void UpdateView()
		{
			var hasRestriction = false;

			_view.PopulateShiftCategories();
			_view.PopulateAbsences();
			_view.PopulateDayOffs();
			_view.PopulateActivities();

			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (!(persistableScheduleData is IPreferenceDay)) continue;
				var preferenceRestriction = ((IPreferenceDay) persistableScheduleData).Restriction;

				if (preferenceRestriction != null)
				{
					hasRestriction = true;

					if (preferenceRestriction.ShiftCategory != null)
					{
						_view.UpdateShiftCategory(preferenceRestriction.ShiftCategory);
						_view.UpdateShiftCategoryExtended(preferenceRestriction.ShiftCategory);
						_view.ClearDayOff();
						_view.ClearAbsence();
					}

					if (preferenceRestriction.Absence != null)
					{
						_view.UpdateAbsence(preferenceRestriction.Absence);
						_view.ClearShiftCategory();
						_view.ClearShiftCategoryExtended();
						_view.ClearDayOff();
						_view.ClearActivity();
					}

					if (preferenceRestriction.DayOffTemplate != null)
					{
						_view.UpdateDayOff(preferenceRestriction.DayOffTemplate);
						_view.ClearShiftCategory();
						_view.ClearShiftCategoryExtended();
						_view.ClearAbsence();
						_view.ClearActivity();
					}

					if (preferenceRestriction.ActivityRestrictionCollection.FirstOrDefault() != null)
					{
						var activityRestriction = preferenceRestriction.ActivityRestrictionCollection[0];
						_view.UpdateActivity(activityRestriction.Activity);

						var minLengthActivity = activityRestriction.WorkTimeLimitation.StartTime;
						var maxLengthActivity = activityRestriction.WorkTimeLimitation.EndTime;
						var minStartActivity = activityRestriction.StartTimeLimitation.StartTime;
						var maxStartActivity = activityRestriction.StartTimeLimitation.EndTime;
						var minEndActivity = activityRestriction.EndTimeLimitation.StartTime;
						var maxEndActivity = activityRestriction.EndTimeLimitation.EndTime;
						_view.UpdateActivityTimes(minLengthActivity, maxLengthActivity, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity);

						_view.ClearDayOff();
						_view.ClearAbsence();
					}

					var minLength = preferenceRestriction.WorkTimeLimitation.StartTime;
					var maxLength = preferenceRestriction.WorkTimeLimitation.EndTime;
					var minStart = preferenceRestriction.StartTimeLimitation.StartTime;
					var maxStart = preferenceRestriction.StartTimeLimitation.EndTime;
					var minEnd = preferenceRestriction.EndTimeLimitation.StartTime;
					var maxEnd = preferenceRestriction.EndTimeLimitation.EndTime;

					_view.UpdateTimesExtended(minLength, maxLength, minStart, maxStart, minEnd, maxEnd);
					_view.UpdateMustHave(preferenceRestriction.MustHave);
				}

				break;
			}

			if (!hasRestriction)
			{
				_view.ClearShiftCategory();
				_view.ClearShiftCategoryExtended();
				_view.ClearAbsence();
				_view.ClearDayOff();
				_view.ClearActivity();	
			}	
		}	
	}
}
