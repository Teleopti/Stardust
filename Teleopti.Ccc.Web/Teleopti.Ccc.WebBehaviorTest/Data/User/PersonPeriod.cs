using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PersonPeriod : IUserSetup
	{
		public ITeam Team;
		public PersonContract PersonContract;
		public Domain.AgentInfo.PersonPeriod ThePersonPeriod;
		public IContractSchedule ContractSchedule = DataContext.Data().Data<ContractScheduleWith2DaysOff>().ContractSchedule;
		public IContractSetup Contract = DataContext.Data().Data<CommonContract>();
		private DateTime StartDate;

		public PersonPeriod() : this(TestData.CommonTeam) { }
		public PersonPeriod(ITeam team) : this(team, new DateTime(2001, 1, 1)) { }
		public PersonPeriod(ITeam team, DateTime startDate)
		{
			Team = team;
			StartDate = startDate;
		}

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			PersonContract = new PersonContract(Contract.Contract,
												TestData.PartTimePercentageOne,
												ContractSchedule);
			ThePersonPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(StartDate), 
											PersonContract,
											Team);
			user.AddPersonPeriod(ThePersonPeriod);
		}
	}
}