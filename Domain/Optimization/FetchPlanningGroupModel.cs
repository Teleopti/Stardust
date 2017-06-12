using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchPlanningGroupModel : IFetchPlanningGroupModel
	{
		private readonly IPlanningGroupRepository _planningGroupRepository;
		private readonly PlanningGroupMapper _planningGroupMapper;

		public FetchPlanningGroupModel(IPlanningGroupRepository planningGroupRepository, PlanningGroupMapper planningGroupMapper)
		{
			_planningGroupRepository = planningGroupRepository;
			_planningGroupMapper = planningGroupMapper;
		}

		public IEnumerable<PlanningGroupModel> FetchAll()
		{
			var all = _planningGroupRepository.LoadAll();

			var result = all.Select(planningGroup => _planningGroupMapper.ToModel(planningGroup)).ToList();

			return result;
		}

		public PlanningGroupModel Fetch(Guid id)
		{
			var planningGroup = _planningGroupRepository.Get(id);
			if (planningGroup == null)
				throw new ArgumentException($"Cannot find PlanningGroup with Id {id}");

			return _planningGroupMapper.ToModel(planningGroup);
		}
	}
}