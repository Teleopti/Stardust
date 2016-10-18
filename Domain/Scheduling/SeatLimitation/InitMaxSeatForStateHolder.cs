using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class InitMaxSeatForStateHolder : IInitMaxSeatForStateHolder
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;

		public InitMaxSeatForStateHolder(Func<ISchedulerStateHolder> stateHolder, MaxSeatSkillDataFactory maxSeatSkillDataFactory)
		{
			_stateHolder = stateHolder;
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
		}

		public void Execute()
		{
			var stateHolder = _stateHolder();
			var result = _maxSeatSkillDataFactory.Create(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.PersonsInOrganization, stateHolder.RequestedScenario,stateHolder.SchedulingResultState.PersonsInOrganization);
			result.AllMaxSeatSkills().ForEach(s => stateHolder.SchedulingResultState.AddSkills(s));
			result.AllMaxSeatSkillDaysPerSkill().ForEach(kvp => stateHolder.SchedulingResultState.SkillDays.Add(kvp));
		}
	}
}