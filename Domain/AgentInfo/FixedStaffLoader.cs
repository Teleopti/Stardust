using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class FixedStaffLoader : IFixedStaffLoader
	{
		private readonly IPersonRepository _personRepository;

		public FixedStaffLoader(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period)
		{
			//RK: needs to be loading schedule period here for scheduling. not needed for publishing I think...
			//fix soon!
			var allPeople = _personRepository.FindPeopleInOrganization(period, false).ToList();
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