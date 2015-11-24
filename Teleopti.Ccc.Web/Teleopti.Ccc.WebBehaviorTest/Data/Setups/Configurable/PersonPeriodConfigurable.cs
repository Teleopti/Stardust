using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonPeriodConfigurable : TestCommon.TestData.Setups.Configurable.PersonPeriodConfigurable
	{
		public override void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			if (Contract == null)
			{
				var contract = new ContractConfigurable();
				DataMaker.Data().Apply(contract);
				Contract = contract.Name;
			}

			if (PartTimePercentage == null)
			{
				var partTimePercentage = new PartTimePercentageConfigurable();
				DataMaker.Data().Apply(partTimePercentage);
				PartTimePercentage = partTimePercentage.Name;
			}

			if (ContractSchedule == null)
			{
				var contractSchedule = new ContractScheduleConfigurable();
				DataMaker.Data().Apply(contractSchedule);
				ContractSchedule = contractSchedule.ContractSchedule.Description.Name;
			}

			if (Team == null)
			{
				var team = new TeamConfigurable();
				DataMaker.Data().Apply(team);
				Team = team.Name;
			}

			ExternalLogon = user.Name.ToString();
			ExternalLogonDataSourceId = PhoneStateStepDefinitions.DataSourceId;

			base.Apply(uow, user, cultureInfo);
		}
	}
}