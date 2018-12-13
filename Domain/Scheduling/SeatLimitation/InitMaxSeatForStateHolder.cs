using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class InitMaxSeatForStateHolder
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly MaxSeatSkillDataFactory _maxSeatSkillDataFactory;

		public InitMaxSeatForStateHolder(Func<ISchedulerStateHolder> stateHolder, MaxSeatSkillDataFactory maxSeatSkillDataFactory)
		{
			_stateHolder = stateHolder;
			_maxSeatSkillDataFactory = maxSeatSkillDataFactory;
		}

		public void Execute(int intervalLength)
		{
			var stateHolder = _stateHolder();
			var result = _maxSeatSkillDataFactory.Create(stateHolder.RequestedPeriod.DateOnlyPeriod, stateHolder.SchedulingResultState.LoadedAgents, stateHolder.RequestedScenario,stateHolder.SchedulingResultState.LoadedAgents, intervalLength);
			//lägger på
			result.AllMaxSeatSkills().ForEach(s => stateHolder.SchedulingResultState.AddSkills(s));
			result.AllMaxSeatSkillDaysPerSkill().ForEach(kvp => stateHolder.SchedulingResultState.SkillDays.Add(kvp.Key, kvp.Value));
		}
	}
}