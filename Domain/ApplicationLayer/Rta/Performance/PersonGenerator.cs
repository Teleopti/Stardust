using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public interface IPersonGenerator
	{
		PersonDataForLoadTest Generate(int count);
		void Clear(int count);
	}

	public class PersonDataForLoadTest
	{
		public IEnumerable<PersonWithExternalLogon> Persons { get; set; }
		public Guid TeamId { get; set; }
	}

	public class PersonWithExternalLogon
	{
		public string ExternalLogOn { get; set; }
	}

	public class PersonGenerator : IPersonGenerator
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		private readonly IScheduleGenerator _scheduleGenerator;
		private readonly INow _now;

		public PersonGenerator(ICurrentUnitOfWork unitOfWork, 
			IPersonRepository personRepository,
			ISiteRepository siteRepository,
			ITeamRepository teamRepository,
			IPartTimePercentageRepository partTimePercentageRepository,
			IContractRepository contractRepository,
			IContractScheduleRepository contractScheduleRepository,
			IExternalLogOnRepository externalLogOnRepository,
			IScheduleGenerator scheduleGenerator, INow now)
		{
			_unitOfWork = unitOfWork;
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_externalLogOnRepository = externalLogOnRepository;
			_scheduleGenerator = scheduleGenerator;
			_now = now;
		}

		public PersonDataForLoadTest Generate(int count)
		{
			var date = new DateOnly(_now.UtcDateTime());
			var site = new Site("site");
			_siteRepository.Add(site);
			var team = new Team {Site = site, Description = new Description("team")};
			_teamRepository.Add(team);
			var contract = new Contract("c");
			_contractRepository.Add(contract);
			var partTimePercentage = new PartTimePercentage("p");
			_partTimePercentageRepository.Add(partTimePercentage);
			var contractSchedule = new ContractSchedule("cs");
			_contractScheduleRepository.Add(contractSchedule);
			var generatedPersonData = new List<PersonWithExternalLogon>();
			for (var i = 0; i < count; i++)
			{
				var externalLogOn = new ExternalLogOn(i, i, Convert.ToString(i), Convert.ToString(i), true) {DataSourceId = 6};
				_externalLogOnRepository.Add(externalLogOn);
				var person = new Person();
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
				var personPeriod = new PersonPeriod(date,
					new PersonContract(contract, partTimePercentage, contractSchedule), team);
				personPeriod.AddExternalLogOn(externalLogOn);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);
				_unitOfWork.Current().PersistAll();
				_scheduleGenerator.Generate(person.Id.GetValueOrDefault(), date);

				generatedPersonData.Add(new PersonWithExternalLogon
				{
					ExternalLogOn = externalLogOn.AcdLogOnName
				});
			}
			return new PersonDataForLoadTest { Persons = generatedPersonData, TeamId = team.Id.GetValueOrDefault() };
		}

		public void Clear(int count)
		{
			var personToRemove = _personRepository.LoadAll().OrderByDescending(x => x.UpdatedOn).Take(count).ToList();
			var singlePerson = personToRemove.First();
			var team = singlePerson.MyTeam(new DateOnly(_now.UtcDateTime()));
			_siteRepository.Remove(team.Site);
			_teamRepository.Remove(team);
			var personPeriodTemplate = singlePerson.PersonPeriodCollection.Single();
			_contractRepository.Remove(personPeriodTemplate.PersonContract.Contract);
			_partTimePercentageRepository.Remove(personPeriodTemplate.PersonContract.PartTimePercentage);
			_contractScheduleRepository.Remove(personPeriodTemplate.PersonContract.ContractSchedule);
			foreach (var person in personToRemove)
			{
				var personPeriod = person.PersonPeriodCollection.Single();
				_externalLogOnRepository.Remove(personPeriod.ExternalLogOnCollection.Single());
				person.DeletePersonPeriod(personPeriod);
				_personRepository.Remove(person);
			}
			_unitOfWork.Current().PersistAll();
		}
	}
}