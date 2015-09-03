using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class NextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public NextPlanningPeriodProvider(IPlanningPeriodRepository planningPeriodRepository, INow now, ICurrentBusinessUnit currentBusinessUnit)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_now = now;
			_currentBusinessUnit = currentBusinessUnit;
		}
		
		public IPlanningPeriod Current()
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();
			var result =
				foundPlanningPeriods.Where(x => x.Range.StartDate > _now.LocalDateOnly())
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			if (planningPeriodNotFound(result))
			{
				var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
				var planningPeriod = new PlanningPeriod(planningPeriodSuggestion);
				_planningPeriodRepository.Add(planningPeriod);
				return planningPeriod;
			}
			return result;
		}

		public IPlanningPeriod Next(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation)
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();
			var current =
				foundPlanningPeriods.Where(x => x.Range.StartDate > _now.LocalDateOnly())
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			var next = foundPlanningPeriods.Where(x => x.Range.StartDate >= current.Range.EndDate.AddDays(1))
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			
			if (planningPeriodNotFound(next))
			{
				//refactor
				var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
				var planningPeriod = new PlanningPeriod(planningPeriodSuggestion);
				planningPeriod.ChangeRange(schedulePeriodForRangeCalculation);
				_planningPeriodRepository.Add(planningPeriod);
				return planningPeriod;
			}
			return next;
		}

		private static bool planningPeriodNotFound(IPlanningPeriod result)
		{
			return result == null;
		}
	}
}