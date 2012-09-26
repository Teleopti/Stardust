using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AlertStepDefinition
	{
		[Then(@"I should see only one alert containing '(.*)'")]
		public void ThenIShouldSeeOnlyOneAlertContaining(string activity)
		{
			EventualAssert.That(() =>
					 Browser.Current.Elements.Filter(Find.ByClass("alertLoggerItem")).Exists(Find.ByText(t => t.Contains(activity))),
					 Is.True);
			Browser.Current.Elements.Filter(Find.ByClass("alertLoggerItem")).Count.Should().Be.EqualTo(1);
		}

		[Then(@"I should see one alert")]
		public void ThenIShouldSeeAnAlert()
		{
			Browser.Current.Elements.Filter(Find.ByClass("alertLoggerItem")).Count.Should().Be.EqualTo(1);
		}
	}
}