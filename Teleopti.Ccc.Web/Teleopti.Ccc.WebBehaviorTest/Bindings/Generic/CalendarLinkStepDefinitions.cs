using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class CalendarLinkStepDefinitions
	{
		[Given(@"I have shared calendar")]
		public void GivenIHaveSharedCalendar()
		{
			var calendarLinkConfigurable = new CalendarLinkConfigurable
				{
					IsActive = true
				};
			UserFactory.User().Setup(calendarLinkConfigurable);
		}

		[Then(@"I should see '(.*)' active")]
		public void ThenIShouldSeeActive(CssClass cssClass)
		{
			Browser.Interactions.AssertExists(string.Format(".{0} .active", cssClass.Name));
		}

		[Then(@"I should see a sharing link")]
		public void ThenIShouldSeeASharingLink()
		{
			Browser.Interactions.AssertExists(string.Format(".{0}", "calendar-url"));
		}

	}
}