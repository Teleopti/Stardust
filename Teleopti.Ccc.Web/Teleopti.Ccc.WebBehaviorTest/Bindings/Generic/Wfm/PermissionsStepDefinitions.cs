using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class PermissionsStepDefinitions
	{
		[When(@"I create a role '(.*)'")]
		public void WhenICreateARole(string p0)
		{
			Browser.Interactions.Javascript("var inp = document.querySelector('.role-input');" +
			                                "inp.value = '"+p0+"';" +
			                                "angular.element(inp).triggerHandler('input');" +
			                                "var form = document.querySelector('.role-nav form');" +
			                                "var ngForm = angular.element(form);" +
			                                "ngForm.triggerHandler('submit');");
		}

		[Then(@"I should see a role '(.*)' in the list")]
		public void ThenIShouldSeeARoleInTheList(string p0)
		{
			Browser.Interactions.AssertAnyContains(".wfm-list-item", p0);
		}


	}
}