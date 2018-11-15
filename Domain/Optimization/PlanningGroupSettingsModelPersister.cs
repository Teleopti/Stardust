using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettingsModelPersister : IPlanningGroupSettingsModelPersister
	{
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public PlanningGroupSettingsModelPersister(IPlanningGroupSettingsRepository planningGroupSettingsRepository, FilterMapper filterMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
			_filterMapper = filterMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public void Persist(PlanningGroupSettingsModel model)
		{
			PlanningGroup planningGroup = null;
			if (model.PlanningGroupId.HasValue)
				planningGroup = _planningGroupRepository.Get(model.PlanningGroupId.Value);

			if (model.Id == Guid.Empty)
			{
				PlanningGroupSettings planningGroupSettings;
				if (model.Default)
				{
					planningGroupSettings = PlanningGroupSettings.CreateDefault(planningGroup);
				}
				else
				{
					var allSettingses = _planningGroupSettingsRepository.LoadAllByPlanningGroup(planningGroup);
					model.Priority = allSettingses.IsEmpty() ? 0 : allSettingses.Max(x => x.Priority) + 1;
					planningGroupSettings = new PlanningGroupSettings(planningGroup);
				}
				setProperies(planningGroupSettings, model);
				_planningGroupSettingsRepository.Add(planningGroupSettings);
			}
			else
			{
				var planningGroupSettings = _planningGroupSettingsRepository.Get(model.Id);
				setProperies(planningGroupSettings, model);
			}
		}

		public void Delete(Guid id)
		{
			var planningGroupSettings = _planningGroupSettingsRepository.Get(id);
			if (planningGroupSettings != null)
				_planningGroupSettingsRepository.Remove(planningGroupSettings);
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
			planningGroupSettings.Priority = planningGroupSettingsModel.Priority;
			planningGroupSettings.FullWeekendsOff = new MinMax<int>(planningGroupSettingsModel.MinFullWeekendsOff, planningGroupSettingsModel.MaxFullWeekendsOff);
			planningGroupSettings.WeekendDaysOff = new MinMax<int>(planningGroupSettingsModel.MinWeekendDaysOff, planningGroupSettingsModel.MaxWeekendDaysOff);

			planningGroupSettings.ClearFilters();
			foreach (var filter in planningGroupSettingsModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				planningGroupSettings.AddFilter(filter);
			}
		}
	}
}