using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTimeHandler : IHandleEvent<AddOverTimeEvent>, IRunOnStardust
	{
		private readonly ScheduleOvertime _scheduleOvertime;

		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly ISkillRepository _skillRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly INow _now;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public AddOverTimeHandler(ScheduleOvertime scheduleOvertime,
								  INow now, IFillSchedulerStateHolder fillSchedulerStateHolder,
								  Func<ISchedulerStateHolder> schedulerStateHolder,
								  IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
								  IUpdateStaffingLevelReadModel updateStaffingLevelReadModel,
								  ISkillRepository skillRepository,
								  IRuleSetBagRepository ruleSetBagRepository, IStardustJobFeedback stardustJobFeedback, 
								  IScheduleDifferenceSaver scheduleDifferenceSaver, ICurrentUnitOfWork currentUnitOfWork)
		{
			_scheduleOvertime = scheduleOvertime;
			_now = now;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_skillRepository = skillRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_stardustJobFeedback = stardustJobFeedback;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(AddOverTimeEvent @event)
		{
			IMultiplicatorDefinitionSet multi;
			if (@event.OvertimeType.HasValue)
				multi = _multiplicatorDefinitionSetRepository.Load(@event.OvertimeType.Value); //guess it needs to be loaded in stateholder anyway or?
			else
				multi = _multiplicatorDefinitionSetRepository.LoadAll()[0];
			

			IRuleSetBag bag = null;
			if (@event.ShiftBagToUse.HasValue)
				bag = _ruleSetBagRepository.Get(@event.ShiftBagToUse.Value);

			var loadedTime = _now.UtcDateTime();
			var startTime = TimeSpan.FromHours(loadedTime.AddHours(1).Hour);

			var stateHolder = _schedulerStateHolder();
			var localDateOnly = _now.LocalDateOnly();
			_fillSchedulerStateHolder.Fill(stateHolder, null, null, null, new DateOnlyPeriod(localDateOnly.AddDays(-8), localDateOnly.AddDays(8)));

			var selectedTimePeriod = new TimePeriod(@event.OvertimeDurationMin, @event.OvertimeDurationMax); // how long the overtime should be
			var scheduleTag = new NullScheduleTag();
			var selectedSpecificTimePeriod = new TimePeriod(startTime, startTime.Add(TimeSpan.FromHours(24))); // when it can start earliest, and end latest
			

			var scheduleDays = stateHolder.Schedules.SchedulesForDay(localDateOnly).ToList();
			if (!scheduleDays.Any()) return;

			
			var activities = new List<IActivity>();
			var skills = new List<ISkill>();
			foreach (var skillId in @event.Skills)
			{
				var skill = _skillRepository.Get(skillId);
				if (skill == null) continue;
				skills.Add(skill);
				var act = skill.Activity;
				if (!activities.Contains(act))
				{
					activities.Add(act);
				}
			}
			
			foreach (var activity in activities)
			{
				var skillsForActivity = skills.Where(x => x.Activity == activity);
				var filteredPersons = FilterPersonsOnSkill.Filter(localDateOnly, stateHolder.AllPermittedPersons, skillsForActivity);
				var scheduleDaysOnPersonsWithSkills = scheduleDays.Where(scheduleDay => filteredPersons.Select(pers => pers.Id).Contains(scheduleDay.Person.Id)).ToList();

				_stardustJobFeedback.SendProgress($"Try to add overtime for activity {activity.Name}");
				var overTimePreferences = new OvertimePreferences
				{
					SelectedTimePeriod = selectedTimePeriod,
					ScheduleTag = scheduleTag,
					AvailableAgentsOnly = true,
					SelectedSpecificTimePeriod = selectedSpecificTimePeriod,
					OvertimeType = multi,
					SkillActivity = activity,
					ShiftBagToUse = bag
				};

				_scheduleOvertime.Execute(overTimePreferences, new NoSchedulingProgress(), scheduleDaysOnPersonsWithSkills);
			}


			var changes = stateHolder.Schedules.DifferenceSinceSnapshot();
			foreach (var change in changes)
			{
				if (change.CurrentItem == null || change.CurrentItem.GetType() != typeof(PersonAssignment)) continue;
				IDifferenceCollection<IPersistableScheduleData> changeCollection = new DifferenceCollection<IPersistableScheduleData>() {change};
				_scheduleDifferenceSaver.SaveChanges(changeCollection, (IUnvalidatedScheduleRangeUpdate) stateHolder.Schedules[change.CurrentItem.Person]);
			}

			_currentUnitOfWork.Current().PersistAll();

			var resCalcData = stateHolder.SchedulingResultState.ToResourceOptimizationData(true, false);
			var period = new DateTimePeriod(loadedTime.AddHours(-25), loadedTime.AddHours(25));
			var periodDateOnly = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
			_updateStaffingLevelReadModel.UpdateFromResourceCalculationData(period, resCalcData, periodDateOnly, loadedTime);
		}
	}
}
