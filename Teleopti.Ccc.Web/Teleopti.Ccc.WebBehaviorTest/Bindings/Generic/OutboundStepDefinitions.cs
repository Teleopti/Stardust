using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

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
