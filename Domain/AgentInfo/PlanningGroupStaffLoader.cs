using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PlanningGroupStaffLoader : IPlanningGroupStaffLoader
	{
		private readonly IPersonRepository _personRepository;

		public PlanningGroupStaffLoader(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period, PlanningGroup planningGroup)
		{
			var result = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
			var peopleToSchedule = result.FixedStaffPeople(period);
			return new PeopleSelection(result, peopleToSchedule);
		}

		public int NumberOfAgents(DateOnlyPeriod period, PlanningGroup planningGroup)
		{
			return planningGroup != null ? _personRepository.CountPeopleInPlanningGroup(planningGroup, period) : 0;
		}

		public IList<Guid> LoadPersonIds(DateOnlyPeriod period, PlanningGroup planningGroup)
		{
			if (planningGroup == null) throw new ArgumentNullException(nameof(planningGroup));
			return _personRepository.FindPeopleIdsInPlanningGroup(planningGroup, period);
		}
	}
}