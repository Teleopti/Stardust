using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModelPersister : IDayOffRulesModelPersister
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public DayOffRulesModelPersister(IDayOffRulesRepository dayOffRulesRepository, FilterMapper filterMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_filterMapper = filterMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public void Persist(DayOffRulesModel model)
		{
			IPlanningGroup planningGroup = null;
			if (model.PlanningGroupId.HasValue)
				planningGroup = _planningGroupRepository.Get(model.PlanningGroupId.Value);

			if (model.Id == Guid.Empty)
			{
				var dayOffRules = model.Default ?
					PlanningGroupSettings.CreateDefault(planningGroup) :
					new PlanningGroupSettings(planningGroup);
				setProperies(dayOffRules, model);
				_dayOffRulesRepository.Add(dayOffRules);
			}
			else
			{
				var dayOffRules = _dayOffRulesRepository.Get(model.Id);
				setProperies(dayOffRules, model);
			}
		}

		public void Delete(Guid id)
		{
			var dayOffRule = _dayOffRulesRepository.Get(id);
			if (dayOffRule != null)
				_dayOffRulesRepository.Remove(dayOffRule);
		}

		private void setProperies(PlanningGroupSettings planningGroupSettings, DayOffRulesModel dayOffRulesModel)
		{
			planningGroupSettings.DayOffsPerWeek = new MinMax<int>(dayOffRulesModel.MinDayOffsPerWeek, dayOffRulesModel.MaxDayOffsPerWeek);
			planningGroupSettings.ConsecutiveDayOffs = new MinMax<int>(dayOffRulesModel.MinConsecutiveDayOffs, dayOffRulesModel.MaxConsecutiveDayOffs);
			planningGroupSettings.ConsecutiveWorkdays = new MinMax<int>(dayOffRulesModel.MinConsecutiveWorkdays, dayOffRulesModel.MaxConsecutiveWorkdays);
			planningGroupSettings.Name = dayOffRulesModel.Name;
			planningGroupSettings.BlockFinderType = dayOffRulesModel.BlockFinderType;
			planningGroupSettings.BlockSameStartTime = dayOffRulesModel.BlockSameStartTime;
			planningGroupSettings.BlockSameShiftCategory = dayOffRulesModel.BlockSameShiftCategory;
			planningGroupSettings.BlockSameShift = dayOffRulesModel.BlockSameShift;

			planningGroupSettings.ClearFilters();
			foreach (var filter in dayOffRulesModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				planningGroupSettings.AddFilter(filter);
			}
		}
	}
}