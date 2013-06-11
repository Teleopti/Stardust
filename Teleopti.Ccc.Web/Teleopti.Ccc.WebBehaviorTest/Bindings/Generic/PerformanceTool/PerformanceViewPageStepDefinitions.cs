using System;
using System.Dynamic;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.PerformanceTool
{
	[Binding]
	public class PerformanceViewPageStepDefinitions
	{
		[When(@"I select scenario '(.*)'")]
		public void WhenISelectScenario(string scenario)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery(".scenario-selector", scenario);
		}

		[When(@"I input a configuration in json format")]
		public void WhenIInputAConfigurationInJsonFormat()
		{
			var configuration = new
				{
					PersonIds = new[]
						{
							UserFactory.User().Person.Id.Value.ToString()
						},
					AbsenceId = new[]
						{
							UserFactory.User().UserData<AbsenceConfigurable>().Absence.Id.Value.ToString()
						},
					DateRange = new
						{
							From = DateTime.Now.Date.ToShortDateString(),
							To = DateTime.Now.Date.AddDays(1).ToShortDateString()
						}
				};
			var value = JsonConvert.SerializeObject(configuration, Formatting.Indented);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".scenario-configuration", value);
		}

		[Then(@"I should see a count of messages received for each applicable model updated")]
		public void ThenIShouldSeeACountOfMessagesReceivedForEachApplicableModelUpdated()
		{
			Browser.Interactions.AssertExists(".message-count:contains('PersonScheduleDayReadModel') .message-target:contains('4')");
			Browser.Interactions.AssertExists(".message-count:contains('PersonScheduleDayReadModel') .message-successes:contains('4')");
		}

		[Then(@"I should see that the test run has finished")]
		public void ThenIShouldSeeThatTheTestRunHasFinished()
		{
			Browser.Interactions.AssertExists(".result-success");
		}

		[Then(@"I should see total run time")]
		public void ThenIShouldSeeTotalRunTime()
		{
			Browser.Interactions.AssertExists(".total-run-time");
		}

		[Then(@"I should see total time to send commands")]
		public void ThenIShouldSeeTotalTimeToSendCommands()
		{
			Browser.Interactions.AssertExists(".total-time-to-send-commands");
		}

		[Then(@"I should see scenarios per second")]
		public void ThenIShouldSeeScenariosPerSecond()
		{
			Browser.Interactions.AssertExists(".scenarios-per-second");
		}
	}
}