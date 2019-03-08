using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class AdjustedAdherenceStepDefinitions
	{
		private readonly DataMakerImpl _data;

		public AdjustedAdherenceStepDefinitions(DataMakerImpl data)
		{
			_data = data;
		}
		
		[Given("there was a technical issue in the adherence states feed between '(.*)' and '(.*)'")]
		public void GivenThereWasATechnicalIssueInTheAdherenceStatesFeedBetween(string from, string to)
		{
		}
		
		[Given(@"the period between '(.*)' and '(.*)' is adjusted to neutral")]
		public void AndThePeriodIsAdjustedToNeutral(string from, string to)
		{
			var period = CurrentTime.MagicParse(from, to);
			_data.Data().Apply(new AdjustedAdherenceSpec
			{
				StartTime = period.StartDateTime,
				EndTime = period.EndDateTime
			});
		}

	}
}