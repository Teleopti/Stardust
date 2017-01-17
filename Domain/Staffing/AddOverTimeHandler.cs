using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public AddOverTimeHandler(ScheduleOvertime scheduleOvertime,
								  INow now, IFillSchedulerStateHolder fillSchedulerStateHolder,
								  Func<ISchedulerStateHolder> schedulerStateHolder,
								  IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
								  IUpdateStaffingLevelReadModel updateStaffingLevelReadModel,
								  ISkillRepository skillRepository,
								  IRuleSetBagRepository ruleSetBagRepository, IStardustJobFeedback stardustJobFeedback, ICurrentUnitOfWork currentUnitOfWork)
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
			_currentUnitOfWork = currentUnitOfWork;
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
			var overTimePreferences = new OvertimePreferences
			{
				SelectedTimePeriod = new TimePeriod(@event.OvertimeDurationMin, @event.OvertimeDurationMax) , // how long the overtime should be
				ScheduleTag = new ScheduleTag(),
				AvailableAgentsOnly = true,
				SelectedSpecificTimePeriod =
					new TimePeriod(_now.UtcDateTime().AddHours(1).Hour,0, _now.UtcDateTime().AddHours(5).Hour,
						0), // when it can start earliest, and end latest
				OvertimeType = multi,
				SkillActivity = act,
				ShiftBagToUse = bag
			};

			var stateHolder = _schedulerStateHolder();
			var loadedTime = _now.UtcDateTime();
			_fillSchedulerStateHolder.Fill(stateHolder, null, null, null, new DateOnlyPeriod(new DateOnly(_now.UtcDateTime().AddDays(-8)), new DateOnly(_now.UtcDateTime().AddDays(8))));
			var filteredPersons = FilterPersonsOnSkill.Filter(new DateOnly(_now.UtcDateTime()), stateHolder.AllPermittedPersons,
				skill);

			var scheduleDays = stateHolder.Schedules.SchedulesForDay(new DateOnly(_now.UtcDateTime().Date)).ToList();
			var scheduleDaysOnPersonsWithSkill = scheduleDays.Where(scheduleDay => filteredPersons.Select(pers => pers.Id).Contains(scheduleDay.Person.Id)).ToList();

			_scheduleOvertime.Execute(overTimePreferences, new NoSchedulingProgress(), scheduleDaysOnPersonsWithSkill);
			_currentUnitOfWork.Current().PersistAll();

			var resCalcData = stateHolder.SchedulingResultState.ToResourceOptimizationData(true, false);

			var period = new DateTimePeriod(_now.UtcDateTime().AddHours(-25), _now.UtcDateTime().AddHours(25));
			var periodDateOnly = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
			//var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));  
			
			_updateStaffingLevelReadModel.UpdateFromResourceCalculationData(period, resCalcData,periodDateOnly, loadedTime);
		}
	}
}
