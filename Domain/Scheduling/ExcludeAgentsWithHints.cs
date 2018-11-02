using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExcludeAgentsWithHints : IExcludeAgentsWithHints
	{
		private readonly CheckScheduleHints _basicCheckScheduleHints;

		public ExcludeAgentsWithHints(CheckScheduleHints basicCheckScheduleHints)
		{
			_basicCheckScheduleHints = basicCheckScheduleHints;
		}

		public IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod)
		{
			var validationResult = _basicCheckScheduleHints.Execute(new ScheduleHintInput(agents, selectedPeriod, true));
			var agentIdsWithHint = validationResult.InvalidResources.Where(x=> x.ValidationErrors.Any(y => y.ErrorResource != nameof(Resources.NoMatchingSchedulePeriod))).Select(x => x.ResourceId);
			var agentsWithoutHints = agents.Where(agent => !agentIdsWithHint.Contains(agent.Id.Value)).ToList();
			return agentsWithoutHints;
		}
	}
	
	
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public interface IExcludeAgentsWithHints
	{
		IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public class NoExcludeAgentsWithHints : IExcludeAgentsWithHints
	{
		public IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod)
		{
			return agents;
		}
	}
}