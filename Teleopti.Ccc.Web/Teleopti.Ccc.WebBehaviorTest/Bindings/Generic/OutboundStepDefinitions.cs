using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	class OutboundStepDefinitions
	{
		[Given(@"there is a campaign with")]
		public void GivenThereIsACampaignWith(Table table)
		{			
			var campaign = table.CreateInstance<OutboundCampaignConfigurable>();
			DataMaker.Data().Apply(campaign);
		}



	}


}
