using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class PersonPeriod : IUserSetup
	{
		public ITeam Team;
		public PersonContract PersonContract;
		public Domain.AgentInfo.PersonPeriod ThePersonPeriod;
		public IContractScheduleSetup ContractSchedule = GlobalDataMaker.Data().Data<CommonContractSchedule>();
		public IContractSetup Contract = GlobalDataMaker.Data().Data<CommonContract>();
		private DateTime StartDate;

		public PersonPeriod() : this(GlobalDataMaker.Data().Data<CommonTeam>().Team) { }
		public PersonPeriod(ITeam team) : this(team, new DateTime(2001, 1, 1)) { }
		public PersonPeriod(ITeam team, DateTime startDate)
		{
			Team = team;
			StartDate = startDate;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			PersonContract = new PersonContract(Contract.Contract,
												GlobalDataMaker.Data().Data<CommonPartTimePercentage>().PartTimePercentage,
												ContractSchedule.ContractSchedule);
			ThePersonPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(StartDate), 
											PersonContract,
											Team);
			user.AddPersonPeriod(ThePersonPeriod);
		}
	}
}