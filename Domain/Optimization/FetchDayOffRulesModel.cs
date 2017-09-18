using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;
		private readonly DayOffRulesMapper _dayOffRulesMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public FetchDayOffRulesModel(IPlanningGroupSettingsRepository planningGroupSettingsRepository, DayOffRulesMapper dayOffRulesMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
			_dayOffRulesMapper = dayOffRulesMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public IEnumerable<PlanningGroupSettingsModel> FetchAllWithoutPlanningGroup()
		{
			var all = _planningGroupSettingsRepository.LoadAllWithoutPlanningGroup();

			if (!all.Any(x => x.Default))
				all.Add(PlanningGroupSettings.CreateDefault());

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}

		public PlanningGroupSettingsModel Fetch(Guid id)
		{
			var dayOffRules = _planningGroupSettingsRepository.Get(id);
			if (dayOffRules == null)
				throw new ArgumentException($"Cannot find PlanningGroupSettings with Id {id}");

			return _dayOffRulesMapper.ToModel(dayOffRules);
		}

		public IEnumerable<PlanningGroupSettingsModel> FetchAllForPlanningGroup(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			var all = _planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup);

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}
	}
}