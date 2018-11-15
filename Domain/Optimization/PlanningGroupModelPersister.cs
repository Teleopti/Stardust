using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupModelPersister : IPlanningGroupModelPersister
	{
		private readonly IPlanningGroupRepository _planningGroupRepository;
		private readonly IPlanningGroupSettingsRepository _planningGroupSettingsRepository;
		private readonly FilterMapper _filterMapper;

		public PlanningGroupModelPersister(IPlanningGroupRepository planningGroupRepository, FilterMapper filterMapper, IPlanningGroupSettingsRepository planningGroupSettingsRepository)
		{
			_planningGroupRepository = planningGroupRepository;
			_filterMapper = filterMapper;
			_planningGroupSettingsRepository = planningGroupSettingsRepository;
		}

		public void Persist(PlanningGroupModel planningGroupModel)
		{
			if (planningGroupModel.Id == Guid.Empty)
			{
				var planningGroup = new PlanningGroup();
				setProperties(planningGroup, planningGroupModel);
				_planningGroupRepository.Add(planningGroup);
				_planningGroupSettingsRepository.Add(PlanningGroupSettings.CreateDefault(planningGroup));
			}
			else
			{
				var planningGroup = _planningGroupRepository.Get(planningGroupModel.Id);
				setProperties(planningGroup, planningGroupModel);
			}
		}

		private void setProperties(PlanningGroup planningGroup, PlanningGroupModel planningGroupModel)
		{
			planningGroup.ChangeName(planningGroupModel.Name);

			planningGroup.ClearFilters();
			foreach (var filter in planningGroupModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				planningGroup.AddFilter(filter);
			}
		}

		public void Delete(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null) return;
			_planningGroupSettingsRepository.RemoveForPlanningGroup(planningGroup);
			_planningGroupRepository.Remove(planningGroup);
		}
	}
}