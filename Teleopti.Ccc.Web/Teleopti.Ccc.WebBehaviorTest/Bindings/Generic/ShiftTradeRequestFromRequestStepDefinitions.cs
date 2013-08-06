using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestFromRequestStepDefinitions
	{
		[Then(@"Details should be closed")]
		public void ThenDetailsShouldBeClosed()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDetailSection.IsDisplayed(), Is.False);
		}

	}
}
