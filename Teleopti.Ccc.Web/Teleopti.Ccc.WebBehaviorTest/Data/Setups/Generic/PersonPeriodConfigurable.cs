using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonPeriodConfigurable : TestCommon.TestData.Generic.PersonPeriodConfigurable
	{
		public PersonPeriodConfigurable()
		{
			Contract = GlobalDataMaker.Data().Data<CommonContract>().Contract.Description.Name;
			PartTimePercentage = GlobalDataMaker.Data().Data<CommonPartTimePercentage>().PartTimePercentage.Description.Name;
			ContractSchedule = GlobalDataMaker.Data().Data<CommonContractSchedule>().ContractSchedule.Description.Name;
			Team = GlobalDataMaker.Data().Data<CommonTeam>().Team.Description.Name;
		}
	}
}