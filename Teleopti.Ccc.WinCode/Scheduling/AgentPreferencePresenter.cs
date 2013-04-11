using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public enum AgentPreferenceExecuteCommand
	{
		Add,
		Edit,
		Remove,
		None
	}

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Remove(IAgentPreferenceRemoveCommand removeCommand)
		{
			if (removeCommand == null) throw new ArgumentNullException("removeCommand");

			removeCommand.Execute();
			UpdateView();
		}

		public void Add(IAgentPreferenceAddCommand addCommand)
		{
			if (addCommand == null) throw new ArgumentNullException("addCommand");

			addCommand.Execute();
			UpdateView();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void Edit(IAgentPreferenceEditCommand editCommand)
		{
			if (editCommand == null) throw new ArgumentNullException("editCommand");

			editCommand.Execute();
			UpdateView();
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

		public AgentPreferenceExecuteCommand CommandToExecute(IShiftCategory shiftCategory, IAbsence absence, IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart, TimeSpan? maxStart,
									TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
									TimeSpan? minStartActivity, TimeSpan? maxStartActivity, TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
									TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity, IAgentPreferenceDayCreator dayCreator)
		{
			if (dayCreator == null) throw new ArgumentNullException("dayCreator");

			var preferenceDay = _scheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().FirstOrDefault();

			var canCreate = dayCreator.CanCreate(shiftCategory, absence, dayOffTemplate, activity, minStart, maxStart, minEnd,
			                                     maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity,
			                                     maxEndActivity, minLengthActivity, maxLengthActivity);

			if (preferenceDay == null && canCreate.Result)
				return AgentPreferenceExecuteCommand.Add;
			
			if(preferenceDay != null && canCreate.Result)
				return AgentPreferenceExecuteCommand.Edit;

			if(preferenceDay != null && !canCreate.Result && canCreate.EmptyError)
				return AgentPreferenceExecuteCommand.Remove;

			return AgentPreferenceExecuteCommand.None;
		}
	}
}
