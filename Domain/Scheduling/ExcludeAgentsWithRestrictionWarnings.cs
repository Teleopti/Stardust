using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExcludeAgentsWithRestrictionWarnings : IExcludeAgentsWithRestrictionWarnings
	{
		private readonly RestrictionNotAbleToBeScheduledReport _restrictionNotAbleToBeScheduledReport;

		public ExcludeAgentsWithRestrictionWarnings(
			RestrictionNotAbleToBeScheduledReport restrictionNotAbleToBeScheduledReport)
		{
			_restrictionNotAbleToBeScheduledReport = restrictionNotAbleToBeScheduledReport;
		}

		[TestLog]
		public virtual IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, bool checkPreferences)
		{
			var restrictionReport =
				_restrictionNotAbleToBeScheduledReport.Create(selectedPeriod, agents, new NoSchedulingProgress(), checkPreferences);

			var agentsNotAbleToScheduleWithRestriction = new List<IPerson>();
			foreach (var restrictionResult in restrictionReport)
			{
				if (restrictionResult.Reason != RestrictionNotAbleToBeScheduledReason.NoIssue &&
					restrictionResult.Reason != RestrictionNotAbleToBeScheduledReason.NoRestrictions)
				{
					agentsNotAbleToScheduleWithRestriction.Add(restrictionResult.Agent);
				}
			}

			return agents.Where(agent => agentsNotAbleToScheduleWithRestriction.All(x => x.Id.Value != agent.Id.Value));
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public interface IExcludeAgentsWithRestrictionWarnings
	{
		IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod,bool checkPreferences);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286)]
	public class NotExcludeAgentsWithRestrictionWarnings : IExcludeAgentsWithRestrictionWarnings
	{
		public IEnumerable<IPerson> Execute(IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod, bool checkPreferences)
		{
			return agents;
		}
	}
}