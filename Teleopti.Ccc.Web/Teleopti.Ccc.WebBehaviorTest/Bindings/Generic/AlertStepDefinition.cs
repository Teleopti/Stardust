using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AlertStepDefinition
	{
		[Then(@"I should see a alert containing '(.*)'")]
		public void ThenIShouldSeeAPopupSayingPhoneIsNewActivity(string activity)
		{
			EventualAssert.That(() =>
					 Browser.Current.Elements.Filter(Find.ByClass("alertLoggerItem")).Exists(Find.ByText(t => t.Contains(activity))),
			       Is.True);
		}
	}
}