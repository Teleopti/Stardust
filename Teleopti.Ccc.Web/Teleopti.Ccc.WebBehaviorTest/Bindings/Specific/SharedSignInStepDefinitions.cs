using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		[Then(@"I should see the sign in page")]
		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should be signed out from MobileReports")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}
	}
}