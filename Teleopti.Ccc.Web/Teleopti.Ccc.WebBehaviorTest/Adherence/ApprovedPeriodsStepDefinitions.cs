using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ApprovedPeriodsStepDefinitions
	{
		private readonly DataMakerImpl _data;

		public ApprovedPeriodsStepDefinitions(DataMakerImpl data)
		{
			_data = data;
		}
		
		[Given(@"'?(.*)'? has an approved period between '(.*)' and '(.*)'")]
		public void GivenIHaveAnApprovedPeriodBetween(string person, string from, string to)
		{
			var period = CurrentTime.MagicParse(from, to);
			_data.Data().Person(person).Apply(new ApprovedPeriodSpec
			{
				StartTime = period.StartDateTime,
				EndTime = period.EndDateTime
			});
		}

		[Given(@"the period between '(.*)' and '(.*)' is approved for '(.*)'")]
		public void GivenThePeriodBetweenAndIsApprovedFor(string from, string to, string person)
			=> GivenIHaveAnApprovedPeriodBetween(person, from, to);
	}
}