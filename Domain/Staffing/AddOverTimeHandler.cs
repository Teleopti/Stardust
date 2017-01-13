using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public AddOverTimeHandler(ScheduleOvertime scheduleOvertime,
			INow now, IFillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
			IActivityRepository activityRepository,
			ISkillRepository skillRepository)
		{
			_scheduleOvertime = scheduleOvertime;
			_now = now;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_activityRepository = activityRepository;
			_skillRepository = skillRepository;
		}

		public void Handle(AddOverTimeEvent @event)
		{
			//guess it needs to be loaded in stateholder anyway or?
			var multi = _multiplicatorDefinitionSetRepository.Load(@event.OvertimeType);
			var skill = _skillRepository.Get(@event.Skills[0]);
			var act = skill.Activity;
			var overTimePreferences = new OvertimePreferences
			{
				SelectedTimePeriod = new TimePeriod(@event.OvertimeDurationMin, @event.OvertimeDurationMax) , // how long the overtime should be
				ScheduleTag = new ScheduleTag(),
				AvailableAgentsOnly = true,
				SelectedSpecificTimePeriod =
					new TimePeriod(_now.UtcDateTime().AddHours(1).Hour, _now.UtcDateTime().Minute, _now.UtcDateTime().AddHours(5).Hour,
						_now.UtcDateTime().Minute), // when it can start earliest, and end latest
				OvertimeType = multi,
				SkillActivity = act
			};

			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(stateHolder, null, null, null, new DateOnlyPeriod(new DateOnly(_now.UtcDateTime().AddDays(-8)), new DateOnly(_now.UtcDateTime().AddDays(8))));

			var scheduleDays = stateHolder.Schedules.SchedulesForDay(new DateOnly(_now.UtcDateTime().Date)).ToList();

			_scheduleOvertime.Execute(overTimePreferences, new NoSchedulingProgress(), scheduleDays);
		}
	}
}
