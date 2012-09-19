using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AsmMessageStepDefinition
	{
        //private MessagePage _page { get { return Pages.Pages.MessagePage; } }

        [Then(@"Message tab should be visible")]
        public void ThenMessageTabShouldBeVisible()
        {
            var page = Browser.Current.Page<PortalPage>();
            EventualAssert.That(() => page.MessageLink.Exists, Is.True);
        }

        [Then(@"Message tab should not be visible")]
        public void ThenMessageTabShouldNotBeVisible()
        {
            var page = Browser.Current.Page<PortalPage>();
            EventualAssert.That(() => page.MessageLink.Exists, Is.False);
        }
	}
}