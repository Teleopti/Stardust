using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonPeriodConfigurable : TestCommon.TestData.Setups.Configurable.PersonPeriodConfigurable
	{
		public PersonPeriodConfigurable()
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
		}
	}
}