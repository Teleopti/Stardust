using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonPeriodConfigurable : IUserSetup
	{
		public DateTime StartDate { get; set; }
		public string Contract { get; set; }
		public string PartTimePercentage { get; set; }
		public string ContractSchedule { get; set; }
		public string Team { get; set; }

		public PersonPeriodConfigurable() {
			Contract = GlobalDataContext.Data().Data<CommonContract>().Contract.Description.Name;
			PartTimePercentage = GlobalDataContext.Data().Data<CommonPartTimePercentage>().PartTimePercentage.Description.Name;
			ContractSchedule = GlobalDataContext.Data().Data<CommonContractSchedule>().ContractSchedule.Description.Name;
			Team = GlobalDataContext.Data().Data<CommonTeam>().Team.Description.Name;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var contractRepository = new ContractRepository(uow);
			var contract = contractRepository.LoadAll().Single(c => c.Description.Name == Contract);

			var partTimePercentageRepository = new PartTimePercentageRepository(uow);
			var partTimePercentage = partTimePercentageRepository.LoadAll().Single(c => c.Description.Name == PartTimePercentage);

			var contractScheduleRepository = new ContractScheduleRepository(uow);
			var contractSchedule = contractScheduleRepository.LoadAll().Single(c => c.Description.Name == ContractSchedule);

			var teamRepository = new TeamRepository(uow);
			var team = teamRepository.LoadAll().Single(c => c.Description.Name == Team);

			var personContract = new PersonContract(contract,
			                                        partTimePercentage,
			                                        contractSchedule);
			var personPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(StartDate),
			                                                     personContract,
			                                                     team);
			user.AddPersonPeriod(personPeriod);
		}
	}
}