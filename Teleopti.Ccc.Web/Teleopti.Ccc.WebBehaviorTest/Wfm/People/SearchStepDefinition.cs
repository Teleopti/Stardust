using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	public sealed class SearchStepDefinition
	{
		[When("Searching for '(.*)'")]
		public void SearchingForText(string searchString)
		{
		}

		[Then("I should see all matches for '(.*)'")]
		public void ShouldSeeAllMatchesForSearch(string searchString)
		{
		}

		[Then("I should see columns")]
		public void IShouldSeeColumns(Table columns)
		{
			var columnNames = columns.AsStrings("Columns");
		}
	}
}
