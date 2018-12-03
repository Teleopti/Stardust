using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class PersonPeriod : IUserSetup
	{
		public ITeam Team;
		public PersonContract PersonContract;
		public Domain.AgentInfo.PersonPeriod ThePersonPeriod;
		public IContractSchedule ContractSchedule;
		public IContract Contract;

		private readonly DateTime _startDate;

		public PersonPeriod(ITeam team) : this(team, new DateTime(2001, 1, 1)) { }
		public PersonPeriod(ITeam team, DateTime startDate)
		{
			Team = team;
			_startDate = startDate;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			if (ContractSchedule == null)
			{
				var contractSchedule = new ContractScheduleConfigurable();
				DataMaker.Data().Apply(contractSchedule);
				ContractSchedule = contractSchedule.ContractSchedule;	
			}
			if (Contract == null)
			{
				var contractConfigurable = new ContractConfigurable();
				DataMaker.Data().Apply(contractConfigurable);
				Contract = contractConfigurable.Contract;
			}

			var partTimePercentage = new PartTimePercentageConfigurable();
			DataMaker.Data().Apply(partTimePercentage);


			PersonContract = new PersonContract(Contract,
												partTimePercentage.PartTimePercentage,
												ContractSchedule);
			ThePersonPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(_startDate), 
											PersonContract,
											Team);
			user.AddPersonPeriod(ThePersonPeriod);
		}
	}
}