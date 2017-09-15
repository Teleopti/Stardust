using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly DayOffRulesMapper _dayOffRulesMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public FetchDayOffRulesModel(IDayOffRulesRepository dayOffRulesRepository, DayOffRulesMapper dayOffRulesMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_dayOffRulesMapper = dayOffRulesMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public IEnumerable<DayOffRulesModel> FetchAllWithoutPlanningGroup()
		{
			var all = _dayOffRulesRepository.LoadAllWithoutPlanningGroup();

			if (!all.Any(x => x.Default))
				all.Add(PlanningGroupSettings.CreateDefault());

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}

		public DayOffRulesModel Fetch(Guid id)
		{
			var dayOffRules = _dayOffRulesRepository.Get(id);
			if (dayOffRules == null)
				throw new ArgumentException($"Cannot find PlanningGroupSettings with Id {id}");

			return _dayOffRulesMapper.ToModel(dayOffRules);
		}

		public IEnumerable<DayOffRulesModel> FetchAllForPlanningGroup(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			var all = _dayOffRulesRepository.LoadAllByPlanningGroup(planningGroup);

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}
	}
}