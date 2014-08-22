﻿using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class PersonPeriod : IUserSetup
	{
		public ITeam Team;
		public PersonContract PersonContract;
		public Domain.AgentInfo.PersonPeriod ThePersonPeriod;
		public IContractSchedule ContractSchedule;
		public IContract Contract = GlobalDataMaker.Data().Data<CommonContract>().Contract;

		private readonly DateTime _startDate;

		public PersonPeriod() : this(GlobalDataMaker.Data().Data<CommonTeam>().Team) { }
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

			PersonContract = new PersonContract(Contract,
												GlobalDataMaker.Data().Data<CommonPartTimePercentage>().PartTimePercentage,
												ContractSchedule);
			ThePersonPeriod = new Domain.AgentInfo.PersonPeriod(new DateOnly(_startDate), 
											PersonContract,
											Team);
			user.AddPersonPeriod(ThePersonPeriod);
		}
	}
}