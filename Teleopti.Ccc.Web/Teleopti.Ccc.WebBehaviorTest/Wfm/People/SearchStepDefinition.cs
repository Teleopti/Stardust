using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	public sealed class SearchStepDefinition
	{
		[Then("I should see all matches for '(.*)'")]
		public void ShouldSeeAllMatchesForSearch(string searchString)
		{
			Browser.Interactions.AssertJavascriptResultContains("return document.querySelectorAll('[data-test-search] [data-test-person]').length === 3", "True"); // 3 because I also exists
		}

		[Then("I should see columns")]
		public void IShouldSeeColumns(Table columns)
		{
			var columnNames = columns.AsStrings("Columns");
			foreach (var header in columnNames)
			{
				Browser.Interactions.AssertJavascriptResultContains("return Array.from(document.querySelectorAll('[role=columnheader]')).map(h => h.textContent.trim()).includes('"+header+"')", "True");
			}
		}
	}
}
