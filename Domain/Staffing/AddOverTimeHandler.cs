using System;
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

		public AddOverTimeHandler(ScheduleOvertime scheduleOvertime,
								  INow now, IFillSchedulerStateHolder fillSchedulerStateHolder,
								  Func<ISchedulerStateHolder> schedulerStateHolder,
								  IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
								  IUpdateStaffingLevelReadModel updateStaffingLevelReadModel,
								  ISkillRepository skillRepository,
								  IRuleSetBagRepository ruleSetBagRepository, IStardustJobFeedback stardustJobFeedback, 
								  IScheduleDifferenceSaver scheduleDifferenceSaver)
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
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(AddOverTimeEvent @event)
		{
			//guess it needs to be loaded in stateholder anyway or?
			var multi = _multiplicatorDefinitionSetRepository.Load(@event.OvertimeType);
			var skill = _skillRepository.Get(@event.Skills[0]);
			_stardustJobFeedback.SendProgress($"Try to add overtime for skill {skill.Name}");
			var act = skill.Activity;
			IRuleSetBag bag = null;
			if (@event.ShiftBagToUse.HasValue)
				bag = _ruleSetBagRepository.Get(@event.ShiftBagToUse.Value);
			var loadedTime = _now.UtcDateTime();
			var startTime = TimeSpan.FromHours(loadedTime.AddHours(1).Hour);
			var overTimePreferences = new OvertimePreferences
			{
				SelectedTimePeriod = new TimePeriod(@event.OvertimeDurationMin, @event.OvertimeDurationMax) , // how long the overtime should be
				ScheduleTag = new NullScheduleTag(),
				AvailableAgentsOnly = true,
				SelectedSpecificTimePeriod =
					new TimePeriod(startTime, startTime.Add(TimeSpan.FromHours(24))), // when it can start earliest, and end latest
				OvertimeType = multi,
				SkillActivity = act,
				ShiftBagToUse = bag
			};

			var stateHolder = _schedulerStateHolder();
			var localDateOnly = _now.LocalDateOnly();
			_fillSchedulerStateHolder.Fill(stateHolder, null, null, null, new DateOnlyPeriod(localDateOnly.AddDays(-8), localDateOnly.AddDays(8)));
			var filteredPersons = FilterPersonsOnSkill.Filter(localDateOnly, stateHolder.AllPermittedPersons,
				skill);

			var scheduleDays = stateHolder.Schedules.SchedulesForDay(localDateOnly).ToList();
			if (!scheduleDays.Any()) return;
			var scheduleDaysOnPersonsWithSkill = scheduleDays.Where(scheduleDay => filteredPersons.Select(pers => pers.Id).Contains(scheduleDay.Person.Id)).ToList();
			
			_scheduleOvertime.Execute(overTimePreferences, new NoSchedulingProgress(), scheduleDaysOnPersonsWithSkill);

			var changes = stateHolder.Schedules.DifferenceSinceSnapshot();
			foreach (var change in changes)
			{
				if(change.CurrentItem == null || change.CurrentItem.GetType() != typeof(PersonAssignment)) continue;
				IDifferenceCollection<IPersistableScheduleData> changeCollection = new DifferenceCollection<IPersistableScheduleData>() {change};
				_scheduleDifferenceSaver.SaveChanges(changeCollection, (IUnvalidatedScheduleRangeUpdate)stateHolder.Schedules[change.CurrentItem.Person]);
			}
			

			var resCalcData = stateHolder.SchedulingResultState.ToResourceOptimizationData(true, false);

			var period = new DateTimePeriod(loadedTime.AddHours(-25), loadedTime.AddHours(25));
			var periodDateOnly = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
			
			_updateStaffingLevelReadModel.UpdateFromResourceCalculationData(period, resCalcData,periodDateOnly, loadedTime);
		}
	}
}
