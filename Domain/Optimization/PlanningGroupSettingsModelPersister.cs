using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettingsModelPersister : IPlanningGroupSettingsModelPersister
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public PlanningGroupSettingsModelPersister(IDayOffRulesRepository dayOffRulesRepository, FilterMapper filterMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_filterMapper = filterMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public void Persist(PlanningGroupSettingsModel model)
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

		private void setProperies(PlanningGroupSettings planningGroupSettings, PlanningGroupSettingsModel planningGroupSettingsModel)
		{
			planningGroupSettings.DayOffsPerWeek = new MinMax<int>(planningGroupSettingsModel.MinDayOffsPerWeek, planningGroupSettingsModel.MaxDayOffsPerWeek);
			planningGroupSettings.ConsecutiveDayOffs = new MinMax<int>(planningGroupSettingsModel.MinConsecutiveDayOffs, planningGroupSettingsModel.MaxConsecutiveDayOffs);
			planningGroupSettings.ConsecutiveWorkdays = new MinMax<int>(planningGroupSettingsModel.MinConsecutiveWorkdays, planningGroupSettingsModel.MaxConsecutiveWorkdays);
			planningGroupSettings.Name = planningGroupSettingsModel.Name;
			planningGroupSettings.BlockFinderType = planningGroupSettingsModel.BlockFinderType;
			planningGroupSettings.BlockSameStartTime = planningGroupSettingsModel.BlockSameStartTime;
			planningGroupSettings.BlockSameShiftCategory = planningGroupSettingsModel.BlockSameShiftCategory;
			planningGroupSettings.BlockSameShift = planningGroupSettingsModel.BlockSameShift;

			planningGroupSettings.ClearFilters();
			foreach (var filter in planningGroupSettingsModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				planningGroupSettings.AddFilter(filter);
			}
		}
	}
}