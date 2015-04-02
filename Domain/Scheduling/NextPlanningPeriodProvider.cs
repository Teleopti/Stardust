using System.Linq;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class NextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IRepository<IPlanningPeriod> _planningPeriodRepository;
		private readonly INow _now;

		public NextPlanningPeriodProvider(IRepository<IPlanningPeriod> planningPeriodRepository, INow now)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_now = now;
		}

		public IPlanningPeriod Current()
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();

			var result = foundPlanningPeriods.FirstOrDefault();
			if (planningPeriodNotFound(result))
			{
				result = new PlanningPeriod(_now);
				_planningPeriodRepository.Add(result);
			}
			return result;
		}

		private static bool planningPeriodNotFound(IPlanningPeriod result)
		{
			return result == null;
		}
	}
}