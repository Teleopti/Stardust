using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PlanningGroupStaffLoader : IPlanningGroupStaffLoader
	{
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IPersonRepository _personRepository;


		public PlanningGroupStaffLoader(IFixedStaffLoader fixedStaffLoader, IPersonRepository personRepository)
		{
			_fixedStaffLoader = fixedStaffLoader;
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period, IPlanningGroup planningGroup)
		{
			if (planningGroup == null)
			{
				return _fixedStaffLoader.Load(period);
			}
			var result = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
			var peopleToSchedule = result.FixedStaffPeople(period);
			return new PeopleSelection(result, peopleToSchedule);
		}

		public int NumberOfAgents(DateOnlyPeriod period, IPlanningGroup planningGroup)
		{
			return planningGroup != null ? _personRepository.CountPeopleInPlanningGroup(planningGroup, period) : 0;
		}

		public IList<Guid> LoadPersonIds(DateOnlyPeriod period, IPlanningGroup planningGroup)
		{
			if (planningGroup == null) throw new ArgumentNullException(nameof(planningGroup));
			return _personRepository.FindPeopleIdsInPlanningGroup(planningGroup, period);
		}
	}
}