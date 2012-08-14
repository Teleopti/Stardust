using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PersonPeriod : IUserSetup
	{
		public ITeam Team;
		public PersonContract PersonContract;
		public Domain.AgentInfo.PersonPeriod ThePersonPeriod;
		public IContractScheduleSetup ContractSchedule = GlobalDataContext.Data().Data<CommonContractSchedule>();
		public IContractSetup Contract = GlobalDataContext.Data().Data<CommonContract>();
		private DateTime StartDate;

		public PersonPeriod() : this(GlobalDataContext.Data().Data<CommonTeam>().Team) { }
		public PersonPeriod(ITeam team) : this(team, new DateTime(2001, 1, 1)) { }
		public PersonPeriod(ITeam team, DateTime startDate)
		{
			Team = team;
			StartDate = startDate;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			PersonContract = new PersonContract(Contract.Contract,
												GlobalDataContext.Data().Data<CommonPartTimePercentage>().PartTimePercentage,
												ContractSchedule.ContractSchedule);
			ThePersonPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(StartDate), 
											PersonContract,
											Team);
			user.AddPersonPeriod(ThePersonPeriod);
		}
	}
}