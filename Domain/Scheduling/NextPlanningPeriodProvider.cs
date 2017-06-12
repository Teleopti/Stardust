using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class NextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly INow _now;

		public NextPlanningPeriodProvider(IPlanningPeriodRepository planningPeriodRepository, INow now)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_now = now;
		}
		
		public IPlanningPeriod Current(IPlanningGroup planningGroup)
		{
			var foundPlanningPeriods = planningGroup != null ? _planningPeriodRepository.LoadForPlanningGroup(planningGroup) : _planningPeriodRepository.LoadAll();
			var result =
				foundPlanningPeriods.Where(x => x.Range.StartDate > _now.ServerDate_DontUse())
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			if (planningPeriodNotFound(result))
			{
				var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
				var planningPeriod = new PlanningPeriod(planningPeriodSuggestion, planningGroup);
				_planningPeriodRepository.Add(planningPeriod);
				return planningPeriod;
			}
			return result;
		}

		public IPlanningPeriod Next(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, IPlanningGroup planningGroup)
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();
			var current =
				foundPlanningPeriods.Where(x => x.Range.StartDate > _now.ServerDate_DontUse())
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			var next = foundPlanningPeriods.Where(x => x.Range.StartDate >= current.Range.EndDate.AddDays(1))
					.OrderBy(y => y.Range.StartDate)
					.FirstOrDefault();
			
			if (planningPeriodNotFound(next))
			{
				//refactor
				var planningPeriodSuggestion = _planningPeriodRepository.Suggestions(_now);
				var planningPeriod = new PlanningPeriod(planningPeriodSuggestion, planningGroup);
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