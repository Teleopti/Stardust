﻿using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SignOutStepDefinitions
	{
		[When(@"I sign out")]
		public void WhenISignOut()
		{
			Browser.Interactions.Click("#signout");
		}

		[When(@"I press back in the web browser")]
		public void WhenIPressBackInTheWebBrowser()
		{
			Browser.Current.WaitForComplete();
			Browser.Current.Back();
		}

	}
}