using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class FixedStaffLoader
	{
		private readonly IPersonRepository _personRepository;

		public FixedStaffLoader(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindPeopleInOrganizationLight(period).ToList();
			var peopleToSchedule =
				allPeople.Where(
					p =>
						p.PersonPeriods(period)
							.Any(
								pp =>
									pp.PersonContract != null && pp.PersonContract.Contract != null &&
									pp.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)).ToList();

			return new PeopleSelection(allPeople, peopleToSchedule);
		}
	}
}