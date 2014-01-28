using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonPeriodConfigurable : TestCommon.TestData.Setups.Configurable.PersonPeriodConfigurable
	{
		public PersonPeriodConfigurable()
		{
			Contract = GlobalDataMaker.Data().Data<CommonContract>().Contract.Description.Name;
			PartTimePercentage = GlobalDataMaker.Data().Data<CommonPartTimePercentage>().PartTimePercentage.Description.Name;
			ContractSchedule = GlobalDataMaker.Data().Data<CommonContractSchedule>().ContractSchedule.Description.Name;
			if (Team == null)
				Team = GlobalDataMaker.Data().Data<CommonTeam>().Team.Description.Name;
		}
	}
}