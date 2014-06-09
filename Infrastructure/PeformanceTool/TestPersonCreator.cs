using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.PeformanceTool
{
	public class TestPersonCreator : ITestPersonCreator
	{
		private readonly IPersonRepository _personRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IExternalLogOnRepository _externalLogOnRepository;
		
		private ThingsForTestPersonsStateHolder _stateHolder;

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

		public void CreatePersons(int numberOfPersons, int datasourceId = 6)
		{
			createThingsNeededForTestPersons();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				addNeededStuffToRepositories();
				for (var i = 0; i < numberOfPersons; i++)
				{
					var person = createPersonWithExternalLogOn(i, datasourceId);
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
			_stateHolder = new ThingsForTestPersonsStateHolder
			{
				Site = site,
				Team = team,
				PartTimePercentage = partTimePercentage,
				Contract = contract,
				ContractSchedule = contractSchedule
			};
		}

		private void addNeededStuffToRepositories()
		{
			_siteRepository.Add(_stateHolder.Site);
			_teamRepository.Add(_stateHolder.Team);
			_partTimePercentageRepository.Add(_stateHolder.PartTimePercentage);
			_contractRepository.Add(_stateHolder.Contract);
			_contractScheduleRepository.Add(_stateHolder.ContractSchedule);
		}

		private IPerson createPersonWithExternalLogOn(int currentIteration, int datasourceId)
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var personPeriod = new PersonPeriod(
				new DateOnly(DateTime.UtcNow),
				new PersonContract(_stateHolder.Contract,
					_stateHolder.PartTimePercentage,
					_stateHolder.ContractSchedule),
				_stateHolder.Team);
			var externalLogOnString = currentIteration.ToString(CultureInfo.InvariantCulture);
			var externalLogOn = new ExternalLogOn(52130, 52130, externalLogOnString, externalLogOnString, true)
			{
				DataSourceId = datasourceId
			};

			_stateHolder.Persons.Add(person);
			_stateHolder.ExternalLogOns.Add(externalLogOn);
			_stateHolder.PersonPeriods.Add(personPeriod);

			_externalLogOnRepository.Add(externalLogOn);
			personPeriod.AddExternalLogOn(externalLogOn);
			person.AddPersonPeriod(personPeriod);
			return person;
		}

		public void RemoveCreatedPersons()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_contractRepository.Remove(_stateHolder.Contract);
				_contractScheduleRepository.Remove(_stateHolder.ContractSchedule);
				_partTimePercentageRepository.Remove(_stateHolder.PartTimePercentage);
				_siteRepository.Remove(_stateHolder.Site);
				_teamRepository.Remove(_stateHolder.Team);
				
				_stateHolder.ExternalLogOns.ForEach(e => _externalLogOnRepository.Remove(e));
				_stateHolder.Persons.ForEach(p => _personRepository.Remove(p));
				uow.PersistAll();
			}
		}
	}
}
