using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class InitMaxSeatForStateHolder : IInitMaxSeatForStateHolder
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly MaxSeatSkillCreator _maxSeatSkillCreator;

		public InitMaxSeatForStateHolder(Func<ISchedulerStateHolder> stateHolder, MaxSeatSkillCreator maxSeatSkillCreator)
		{
			_stateHolder = stateHolder;
			_maxSeatSkillCreator = maxSeatSkillCreator;
		}

		public void Execute()
		{
			var stateHolder = _stateHolder();
			var result = _maxSeatSkillCreator.CreateMaxSeatSkills(stateHolder.RequestedPeriod.DateOnlyPeriod,
				stateHolder.RequestedScenario,
				stateHolder.SchedulingResultState.PersonsInOrganization.ToList(), stateHolder.SchedulingResultState.Skills);
			result.SkillsToAddToStateholder.ForEach(s => stateHolder.SchedulingResultState.AddSkills(s));
			result.SkillDaysToAddToStateholder.ForEach(kvp => stateHolder.SchedulingResultState.SkillDays.Add(kvp));
		}
	}
}