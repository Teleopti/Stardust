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
			var planningGroup = _planningGroupRepository.FindPlanningGroupBySettingId(planningGroupSettingId);
			if (planningGroup == null)
			{
				throw new ArgumentException($"Cannot find PlanningGroupSettings with Id {planningGroupSettingId}");
			}
			var planningGroupSettings = planningGroup.Settings.SingleOrDefault(x => x.Id.HasValue && x.Id.Value == planningGroupSettingId);

			var planningGroupSettingsModel = _planningGroupSettingsMapper.ToModel(planningGroupSettings);
			return planningGroupSettingsModel;
		}

		public IEnumerable<PlanningGroupSettingsModel> FetchAllForPlanningGroup(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			var result = planningGroup.Settings.Select(planningGroupSettings => _planningGroupSettingsMapper.ToModel(planningGroupSettings)).ToList();
			return result;
		}
	}
}