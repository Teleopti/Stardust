using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class CalendarLinkStepDefinitions
	{
		[Given(@"I have shared my calendar")]
		[Given(@"I have shared my calendar before")]
		public void GivenIHaveSharedCalendar()
		{
			var calendarLinkConfigurable = new CalendarLinkConfigurable
			{
				IsActive = true
			};
			UserFactory.User().Setup(calendarLinkConfigurable);
		}

		[Given(@"I have revoked calendar sharing")]
		public void GivenIHaveRevokedCalendarSharing()
		{
			var calendarLinkConfigurable = new CalendarLinkConfigurable
			{
				IsActive = false
			};
			UserFactory.User().Setup(calendarLinkConfigurable);
		}
	}
}