using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class PeopleInOrganization : IAllStaff
	{
		private readonly IPersonRepository _personRepository;
		private readonly ExternalStaffProvider _externalStaffProvider;
		private readonly ISkillRepository _skillRepository;
		private readonly IUserTimeZone _userTimeZone;

		public PeopleInOrganization(IPersonRepository personRepository, 
			ExternalStaffProvider externalStaffProvider,
			ISkillRepository skillRepository,
			IUserTimeZone userTimeZone)
		{
			_personRepository = personRepository;
			_externalStaffProvider = externalStaffProvider;
			_skillRepository = skillRepository;
			_userTimeZone = userTimeZone;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			var externalStaff = _externalStaffProvider.Fetch(_skillRepository.LoadAllSkills(), dateTimePeriod);
			return _personRepository.FindAllAgents(period, false)
				.Union(externalStaff.Select(x => x.CreateExternalAgent()));
		}
	}
}