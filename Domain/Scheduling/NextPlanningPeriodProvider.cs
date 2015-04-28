using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class NextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly INow _now;
		//should this be relplaced by someother togler manager?
		private readonly IScheduleCommandToggle _toggleManager;

		public NextPlanningPeriodProvider(IPlanningPeriodRepository planningPeriodRepository, INow now, IScheduleCommandToggle toggleManager)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_now = now;
			_toggleManager = toggleManager;
		}
		
		public IPlanningPeriod Current()
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();
			var result = foundPlanningPeriods.FirstOrDefault();
			if (planningPeriodNotFound(result))
			{
				var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
				var range = planningPeriodSuggestion.Default(_toggleManager.IsEnabled(Toggles.Wfm_ChangePlanningPeriod_33043));
				var planningPeriod = new PlanningPeriod(range);
				_planningPeriodRepository.Add(planningPeriod);
				return planningPeriod;
			}
			return result;
		}

		public IPlanningPeriod Find(Guid id)
		{
			return _planningPeriodRepository.Load(id);
		}

		public IEnumerable<SchedulePeriodType> SuggestedPeriods()
		{
			var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
			return planningPeriodSuggestion.UniqueSuggestedPeriod;
		}

		private static bool planningPeriodNotFound(IPlanningPeriod result)
		{
			return result == null;
		}
	}
}