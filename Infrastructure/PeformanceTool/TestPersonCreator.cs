using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.PeformanceTool
{
	public interface ITestPersonCreator
	{
		void CreatePersons(int numberOfPersons);
		void RemoveCreatedPersons();
	}

	public class ThingsCreatedForTestPersons
	{
		public IList<IPerson> Persons { get; private set; }
		public IList<IExternalLogOn> ExternalLogOns { get; private set; }
		public IList<IPersonPeriod> PersonPeriods { get; private set; }
		public ISite Site { get; private set; }
		public ITeam Team { get; private set; }
		public IPartTimePercentage PartTimePercentage{ get; private set; }
		public IContract Contract { get; private set; }
		public IContractSchedule ContractSchedule { get; private set; }

		public ThingsCreatedForTestPersons(ISite site,
			ITeam team,
			IPartTimePercentage partTimePercentage,
			IContract contract,
			IContractSchedule contractSchedule)
		{
			Persons = new List<IPerson>();
			ExternalLogOns = new List<IExternalLogOn>();
			PersonPeriods = new List<IPersonPeriod>();

			Site = site;
			Team = team;
			PartTimePercentage = partTimePercentage;
			Contract = contract;
			ContractSchedule = contractSchedule;
		}

	}

	public class TestPersonCreator : ITestPersonCreator
	{
		private readonly IPersonRepository _personRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		
		private ThingsCreatedForTestPersons createdStateHolder;

		public TestPersonCreator(IPersonRepository personRepository,
			ITeamRepository teamRepository,
			ISiteRepository siteRepository, 
			IPartTimePercentageRepository partTimePercentageRepository, 
			IContractRepository contractRepository, 
			IContractScheduleRepository contractScheduleRepository,
			IExternalLogOnRepository externalLogOnRepository)
		{
			_personRepository = personRepository;
			_teamRepository = teamRepository;
			_siteRepository = siteRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_externalLogOnRepository = externalLogOnRepository;
		}


		public void CreatePersons(int numberOfPersons)
		{
			createThingsNeededForTestPersons();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				addNeededStuffToRepositories();
				for (var i = 0; i < numberOfPersons; i++)
				{
					var person = createPersonWithExternalLogOn(i);
					_personRepository.Add(person);
				}
				uow.PersistAll();
			}
		}

		private void createThingsNeededForTestPersons()
		{
			var site = new Site(" ");
			var team = new Team { Description = new Description(" ") };
			site.AddTeam(team);
			var partTimePercentage = new PartTimePercentage(" ");
			var contract = new Contract(" ");
			var contractSchedule = new ContractSchedule(" ");
			createdStateHolder = new ThingsCreatedForTestPersons(site, team, partTimePercentage, contract, contractSchedule);
		}

		private void addNeededStuffToRepositories()
		{
			_siteRepository.Add(createdStateHolder.Site);
			_teamRepository.Add(createdStateHolder.Team);
			_partTimePercentageRepository.Add(createdStateHolder.PartTimePercentage);
			_contractRepository.Add(createdStateHolder.Contract);
			_contractScheduleRepository.Add(createdStateHolder.ContractSchedule);
		}

		private IPerson createPersonWithExternalLogOn(int currentIteration)
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personPeriod = new PersonPeriod(new DateOnly(DateTime.UtcNow),
				new PersonContract(createdStateHolder.Contract, createdStateHolder.PartTimePercentage, createdStateHolder.ContractSchedule),
				createdStateHolder.Team);
			var externalLogOnString = currentIteration.ToString(CultureInfo.InvariantCulture);
			var externalLogOn = new ExternalLogOn(0, 0, externalLogOnString, externalLogOnString, true);

			createdStateHolder.Persons.Add(person);
			createdStateHolder.ExternalLogOns.Add(externalLogOn);
			createdStateHolder.PersonPeriods.Add(personPeriod);

			_externalLogOnRepository.Add(externalLogOn);
			personPeriod.AddExternalLogOn(externalLogOn);
			person.AddPersonPeriod(personPeriod);
			return person;
		}

		public void RemoveCreatedPersons()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_contractRepository.Remove(createdStateHolder.Contract);
				_contractScheduleRepository.Remove(createdStateHolder.ContractSchedule);
				_partTimePercentageRepository.Remove(createdStateHolder.PartTimePercentage);
				_siteRepository.Remove(createdStateHolder.Site);
				_teamRepository.Remove(createdStateHolder.Team);
				
				createdStateHolder.ExternalLogOns.ForEach(e => _externalLogOnRepository.Remove(e));
				createdStateHolder.Persons.ForEach(p => _personRepository.Remove(p));
				uow.PersistAll();
			}
		}
	}
}
