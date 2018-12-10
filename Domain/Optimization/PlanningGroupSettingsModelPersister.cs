using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupSettingsModelPersister : IPlanningGroupSettingsModelPersister
	{
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public PlanningGroupSettingsModelPersister(FilterMapper filterMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_filterMapper = filterMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public void Persist(PlanningGroupSettingsModel model)
		{
			var planningGroup = _planningGroupRepository.Get(model.PlanningGroupId.Value);
			if (model.Id == Guid.Empty)
			{
				var allSettings = planningGroup.Settings;
				model.Priority = allSettings.Max(x => x.Priority) + 1;
				var planningGroupSettings = new PlanningGroupSettings();
				setProperties(planningGroupSettings, model);
				planningGroup.AddSetting(planningGroupSettings);
			}
			else
			{
				var setting = planningGroup.Settings.Single(x => x.Id == model.Id);
				setProperties(setting, model);
			}

			planningGroup.SetGlobalValues(new Percent(model.PreferencePercent / 100d));
		}

		public void Delete(Guid id)
		{
			var planningGroup = _planningGroupRepository.FindPlanningGroupBySettingId(id);
			planningGroup.RemoveSetting(planningGroup.Settings.Single(x => x.Id.HasValue && x.Id.Value==id));
		}


		private void setProperties(PlanningGroupSettings planningGroupSettings, PlanningGroupSettingsModel planningGroupSettingsModel)
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