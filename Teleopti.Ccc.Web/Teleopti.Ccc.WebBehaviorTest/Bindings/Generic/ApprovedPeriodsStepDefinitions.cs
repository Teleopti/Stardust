using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ApprovedPeriodsStepDefinitions
	{
		[Given(@"'?(.*)'? has an approved period between '(.*)' and '(.*)'")]
		public void GivenIHaveAnApprovedPeriodBetween(string person, string from, string to)
		{
			var period = CurrentTime.MagicParse(from, to);
			DataMaker.Person(person).Apply(new ApprovedPeriodConfigurable
			{
				StartTime = period.StartDateTime,
				EndTime = period.EndDateTime
			});
		}

	}
}