using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.PerformanceTool
{
	public interface IPersonGenerator
	{
		void Generate(int count);
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
		private readonly ExternalLogOnRepository _externalLogOnRepository;

		public PersonGenerator(ICurrentUnitOfWork unitOfWork, 
			IPersonRepository personRepository,
			ISiteRepository siteRepository,
			ITeamRepository teamRepository,
			IPartTimePercentageRepository partTimePercentageRepository,
			IContractRepository contractRepository,
			IContractScheduleRepository contractScheduleRepository,
			ExternalLogOnRepository externalLogOnRepository)
		{
			_unitOfWork = unitOfWork;
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_externalLogOnRepository = externalLogOnRepository;
		}

		public void Generate(int count)
		{
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

			for (var i = 0; i < count; i++)
			{
				var externalLogOn = new ExternalLogOn(i, i, Convert.ToString(i), Convert.ToString(i), true);
				_externalLogOnRepository.Add(externalLogOn);
				var person = new Person();
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
				var personPeriod = new PersonPeriod(new DateOnly(2014, 1, 1),
					new PersonContract(contract, partTimePercentage, contractSchedule), team);
				personPeriod.AddExternalLogOn(externalLogOn);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);

				_unitOfWork.Current().PersistAll();
			}
		}
	}
}