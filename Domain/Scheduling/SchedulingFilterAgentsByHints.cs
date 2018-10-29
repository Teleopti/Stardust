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
	public class SchedulingFilterAgentsByHints
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
}