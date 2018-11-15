using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchPlanningGroupSettingsModel : IFetchPlanningGroupSettingsModel
	{
		private readonly PlanningGroupSettingsMapper _planningGroupSettingsMapper;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public FetchPlanningGroupSettingsModel(PlanningGroupSettingsMapper planningGroupSettingsMapper, IPlanningGroupRepository planningGroupRepository)
		{
			_planningGroupSettingsMapper = planningGroupSettingsMapper;
			_planningGroupRepository = planningGroupRepository;
		}

		public PlanningGroupSettingsModel Fetch(Guid planningGroupSettingId)
		{
			var allGroups = _planningGroupRepository.LoadAll();
			foreach (var planningGroup in allGroups)
			{
				foreach (var planningGroupSetting in planningGroup.Settings)
				{
					if (planningGroupSetting.Id.Value == planningGroupSettingId)
						return _planningGroupSettingsMapper.ToModel(planningGroupSetting);
				}
			}

			throw new ArgumentException($"Cannot find PlanningGroupSettings with Id {planningGroupSettingId}");
		}

		public IEnumerable<PlanningGroupSettingsModel> FetchAllForPlanningGroup(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			var result = planningGroup.Settings.Select(planningGroupSettings => _planningGroupSettingsMapper.ToModel(planningGroupSettings)).ToList();
			return result;
		}
	}
}