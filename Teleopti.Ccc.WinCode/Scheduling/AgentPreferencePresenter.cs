using System;
using System.Linq;
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
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AgentPreferencePresenter(IAgentPreferenceView view, IScheduleDay scheduleDay, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_view = view;
			_scheduleDay = scheduleDay;
			_schedulingResultStateHolder = schedulingResultStateHolder;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
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

		public IPreferenceRestriction PreferenceRestriction()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				var preferenceDay = persistableScheduleData as IPreferenceDay;
				if (preferenceDay == null) continue;
				return preferenceDay.Restriction;
			}

			return null;
		}

		public void UpdateView()
		{
			var hasRestriction = false;

			_view.PopulateShiftCategories();
			_view.PopulateAbsences();
			_view.PopulateDayOffs();
			_view.PopulateActivities();

			var preferenceRestriction = PreferenceRestriction();

			if (preferenceRestriction != null)
			{
				hasRestriction = true;

				if (preferenceRestriction.ShiftCategory != null)
				{
					_view.UpdateShiftCategory(preferenceRestriction.ShiftCategory);
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

			_view.UpdateMustHaveText(MustHavesText());

			if (!hasRestriction)
			{
				_view.ClearShiftCategory();
				_view.ClearShiftCategoryExtended();
				_view.ClearAbsence();
				_view.ClearDayOff();
				_view.ClearActivity();	
			}	
		}

		public string MustHavesText()
		{
			return Resources.MustHave + " (" + CurrentMustHaves() + "/" + MaxMustHaves() + ")";	
		}

		public int MaxMustHaves()
		{
			var schedulePeriod = _scheduleDay.Person.VirtualSchedulePeriod(ScheduleDay.DateOnlyAsPeriod.DateOnly);
			if(schedulePeriod.IsValid) return schedulePeriod.MustHavePreference;
			return 0;
		}

		public int CurrentMustHaves()
		{
			var schedulePeriod = _scheduleDay.Person.VirtualSchedulePeriod(ScheduleDay.DateOnlyAsPeriod.DateOnly);
			var person = _scheduleDay.Person;
			int currentMustHaves = 0;

			foreach (var dateOnly in schedulePeriod.DateOnlyPeriod.DayCollection())
			{
				var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);

				foreach (var restrictionBase in scheduleDay.RestrictionCollection())
				{
					var preferenceRestriction = restrictionBase as IPreferenceRestriction;

					if (preferenceRestriction != null && preferenceRestriction.MustHave)
					{
						currentMustHaves++;
					}
				}
			}

			return currentMustHaves;
		}

		public AgentPreferenceExecuteCommand CommandToExecute(IAgentPreferenceData data, IAgentPreferenceDayCreator dayCreator)
		{
			if (dayCreator == null) throw new ArgumentNullException("dayCreator");

			var preferenceDay = _scheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().FirstOrDefault();

			var canCreate = dayCreator.CanCreate(data);

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
