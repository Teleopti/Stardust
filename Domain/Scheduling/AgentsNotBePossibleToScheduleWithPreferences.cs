using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AgentsNotPossibleToScheduleWithPreferences:IAgentsNotPossibleToScheduleWithPreferences
	{
		private readonly RestrictionsAbleToBeScheduled _restrictionsAbleToBeScheduled;

		public AgentsNotPossibleToScheduleWithPreferences(RestrictionsAbleToBeScheduled restrictionsAbleToBeScheduled)
		{
			_restrictionsAbleToBeScheduled = restrictionsAbleToBeScheduled;
		}
		
		public IEnumerable<IPerson> Execute(DateOnlyPeriod selectedPeriod, IPerson[] agents)
		{
			var agentsThatWillNotBePossibleToScheduleWithPreferences = new List<IPerson>();

			var virtualSchedulePeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var agent in agents)
			{
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					virtualSchedulePeriods.Add(agent.VirtualSchedulePeriod(dateOnly));
				}
			}

			foreach (var virtualSchedulePeriod in virtualSchedulePeriods)
			{
				var failReason = _restrictionsAbleToBeScheduled.Execute(virtualSchedulePeriod);
				if (failReason != null && failReason.Reason != RestrictionNotAbleToBeScheduledReason.NoIssue)
					agentsThatWillNotBePossibleToScheduleWithPreferences.Add(failReason.Agent);
			}

			return agentsThatWillNotBePossibleToScheduleWithPreferences;
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	public class
		AgentsNotPossibleToScheduleWithPreferencesNullObject : IAgentsNotPossibleToScheduleWithPreferences
	{
		public IEnumerable<IPerson> Execute(DateOnlyPeriod selectedPeriod, IPerson[] agents)
		{
			return Enumerable.Empty<IPerson>();
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	public interface IAgentsNotPossibleToScheduleWithPreferences
	{
		IEnumerable<IPerson> Execute(DateOnlyPeriod selectedPeriod, IPerson[] agents);
	}
}