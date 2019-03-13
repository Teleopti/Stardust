using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

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
		
		[Given(@"the adjusted period between '(.*)' and '(.*)' is removed")]
		public void AndTheAdjustedPeriodBetweenIsRemoved(string from, string to)
		{
			var period = CurrentTime.MagicParse(from, to);
			_data.Data().Apply(new RemoveAdjustedAdherenceSpec
			{
				StartTime = period.StartDateTime,
				EndTime = period.EndDateTime
			});
		}
	}

	public class RemoveAdjustedAdherenceSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
	
	public class RemoveAdjustedAdherenceSetup : IDataSetup<RemoveAdjustedAdherenceSpec>
	{
		public void Apply(RemoveAdjustedAdherenceSpec spec)
		{
			throw new NotImplementedException();
		}
	}
}