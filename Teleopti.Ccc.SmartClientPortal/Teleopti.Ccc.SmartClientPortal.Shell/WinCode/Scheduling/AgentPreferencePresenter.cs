using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AgentPreferencePresenter
	{
		private readonly IAgentPreferenceView _view;
		private IScheduleDay _scheduleDay;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public AgentPreferencePresenter(IAgentPreferenceView view, IScheduleDay scheduleDay, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_view = view;
			_scheduleDay = scheduleDay;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public IAgentPreferenceView View
		{
			get { return _view; }
		}

	    public IWorkflowControlSet WorkflowControlSet { get { return _scheduleDay.Person.WorkflowControlSet; } }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void RunCommand(IExecutableCommand command)
		{
			InParameter.NotNull("command", command);

			command.Execute();
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
			var schedulePeriod = _scheduleDay.Person.VirtualSchedulePeriod(_scheduleDay.DateOnlyAsPeriod.DateOnly);
			if(schedulePeriod.IsValid) return schedulePeriod.MustHavePreference;
			return 0;
		}

		public int CurrentMustHaves()
		{
            var person = _scheduleDay.Person;
            var schedulePeriod = person.VirtualSchedulePeriod(_scheduleDay.DateOnlyAsPeriod.DateOnly);
			int currentMustHaves = 0;

            var scheduleDays = _schedulingResultStateHolder.Schedules[person].ScheduledDayCollection(schedulePeriod.DateOnlyPeriod);
			foreach (var scheduleDay in scheduleDays)
			{
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

		public IExecutableCommand CommandToExecute(IAgentPreferenceData data, IAgentPreferenceDayCreator dayCreator)
		{
			InParameter.NotNull("dayCreator", dayCreator);

			var preferenceDay = _scheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().FirstOrDefault();

			var canCreate = dayCreator.CanCreate(data);

			if (preferenceDay == null && canCreate.Result)
				return new AgentPreferenceAddCommand(_scheduleDay,data,dayCreator,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);
			
			if(preferenceDay != null && canCreate.Result)
				return new AgentPreferenceEditCommand(_scheduleDay,data,dayCreator,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			if(preferenceDay != null && !canCreate.Result && canCreate.EmptyError)
				return new AgentPreferenceRemoveCommand(_scheduleDay,_schedulingResultStateHolder.Schedules, _scheduleDayChangeCallback);

			return null;
		}
	}
}
