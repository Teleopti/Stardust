using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingFilterAgentsByHints : ISchedulingFilterAgentsByHints
	{
		private readonly CheckScheduleHints _basicCheckScheduleHints;

		public SchedulingFilterAgentsByHints(CheckScheduleHints basicCheckScheduleHints)
		{
			_basicCheckScheduleHints = basicCheckScheduleHints;
		}

		public IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IBlockPreferenceProvider blockPreferenceProvider)
		{
			var validationResult = _basicCheckScheduleHints.Execute(new HintInput(null, agents, selectedPeriod, blockPreferenceProvider, true));
			var agentIdsWithHint = validationResult.InvalidResources.Where(x=> x.ValidationErrors.Any(y => y.ErrorResource != nameof(Resources.NoMatchingSchedulePeriod))).Select(x => x.ResourceId);
			var agentsWithoutHints = agents.Where(agent => !agentIdsWithHint.Contains(agent.Id.Value)).ToList();
			return agentsWithoutHints;
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public interface ISchedulingFilterAgentsByHints
	{
		IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, IBlockPreferenceProvider blockPreferenceProvider);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public class NoSchedulingFilterAgentsByHints : ISchedulingFilterAgentsByHints
	{
		public IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			return agents;
		}
	}
}