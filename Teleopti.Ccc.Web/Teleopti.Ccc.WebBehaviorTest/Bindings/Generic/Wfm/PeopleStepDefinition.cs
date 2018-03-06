using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public sealed class PeopleStepDefinition
	{
		[Given("Person '(.*)' exists")]
		public void PersonXExists(string name)
		{
			DataMaker.Person(name).Apply(new PersonUserConfigurable());
		}

		[When("I select person '(.*)'")]
		public void ISelectPersonX(string name)
		{
			Browser.Interactions.Javascript("Array.from(document.querySelectorAll('[data-test-search][data-test-person]')).forEach(p => {if(p.textContent.includes('"+name+"'))p.click()})");
		}

		[Then("I should see '(.*)' in the workspace")]
		public void IShouldSeeXInTheWorkspace(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"Array.from(document.querySelectorAll('[data-test-workspace] [data-test-person]'))" +
				".findIndex(p => {return p.textContent.includes('" + name + "')}) !== -1", "true");
		}
	}
}