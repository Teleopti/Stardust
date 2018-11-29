using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ReducedSkillsProvider
	{
		private readonly SkillsOnAgentsProvider _skillsOnAgentsProvider;

		public ReducedSkillsProvider(SkillsOnAgentsProvider skillsOnAgentsProvider)
		{
			_skillsOnAgentsProvider = skillsOnAgentsProvider;
		}
		
		public IEnumerable<ISkill> Execute(ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod period)
		{
			var agentSkills = _skillsOnAgentsProvider.Execute(schedulerStateHolder.SchedulingResultState.LoadedAgents, period); //can be optimized to only selected agents
			return agentSkills.Except(schedulerStateHolder.SchedulingResultState.Skills);
		}
	}
}