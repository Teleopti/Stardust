using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchPlanningGroupModel : IFetchPlanningGroupModel
	{
		private readonly IPlanningGroupRepository _planningGroupRepository;
		private readonly PlanningGroupMapper _planningGroupMapper;
		private readonly IPersonRepository _personRepository;

		public FetchPlanningGroupModel(IPlanningGroupRepository planningGroupRepository, PlanningGroupMapper planningGroupMapper, IPersonRepository personRepository)
		{
			_planningGroupRepository = planningGroupRepository;
			_planningGroupMapper = planningGroupMapper;
			_personRepository = personRepository;
		}

		public IEnumerable<PlanningGroupModel> FetchAll()
		{
			var all = _planningGroupRepository.LoadAll();

			var result = all.Select(planningGroup => _planningGroupMapper.ToModel(planningGroup, getAgentsToday(planningGroup))).ToList();

			return result;
		}

		public PlanningGroupModel Fetch(Guid id)
		{
			var planningGroup = _planningGroupRepository.Get(id);
			if (planningGroup == null)
				throw new ArgumentException($"Cannot find PlanningGroup with Id {id}");

			return _planningGroupMapper.ToModel(planningGroup, getAgentsToday(planningGroup));
		}

		private int getAgentsToday(PlanningGroup planningGroup)
		{
			return _personRepository.CountPeopleInPlanningGroup(planningGroup,
				DateOnly.Today.ToDateOnlyPeriod());
		}
	}
}