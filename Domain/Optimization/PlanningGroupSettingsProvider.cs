using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettingsProvider : IPlanningGroupSettingsProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public PlanningGroupSettingsProvider(IPlanningPeriodRepository planningPeriodRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
		}

		public AllSettingsForPlanningGroup Execute(Guid planningPeriodId)
		{
			return _planningPeriodRepository.Get(planningPeriodId).PlanningGroup.Settings;
		}
	}
}