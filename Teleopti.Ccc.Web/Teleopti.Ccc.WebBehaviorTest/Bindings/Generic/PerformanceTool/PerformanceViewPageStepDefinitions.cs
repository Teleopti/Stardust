using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

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

		[When(@"I input a configuration with (.*) scenarios in json format")]
		public void WhenIInputAConfigurationInJsonFormat(int scenarios)
		{
			var configuration = new
				{
					PersonIds = new[]
						{
							DataMaker.Data().MePerson.Id.Value.ToString().ToUpper()
						},
					AbsenceId = DataMaker.Data().UserData<AbsenceConfigurable>().Absence.Id.Value.ToString(),
					DateRange = new
						{
                            From = DateOnlyForBehaviorTests.TestToday.ToShortDateString(),
                            To = DateOnlyForBehaviorTests.TestToday.AddDays(scenarios - 1).ToShortDateString()
						}
				};
			var value = JsonConvert.SerializeObject(configuration, Formatting.Indented);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".scenario-configuration", value);
		}

		[When(@"I input an RTA configuration scenario for '(.*)' in json format on datasource (.*)")]
		public void WhenIInputAnRTAConfigurationScenarioForInJsonFormatOnDatasource(string personName, int datasource)
		{
			var personId = DataMaker.Person(personName).Person.Id.Value;
			var configuration = new
			{
				PlatformTypeId = Guid.Empty,
				SourceId = datasource,
				Persons = new[]
				{
					new
					{
						ExternalLogOn = personName,
						PersonId = personId
					}
				},
				States = new[] {"Phone"},
				ExpectedEndingStateGroup = "Phone",
				Timestamp = CurrentTime.Value()
			};

			var value = JsonConvert.SerializeObject(configuration, Formatting.Indented);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".scenario-configuration", value);
		}

		[When(@"I input an manage adherence configuration scenario for '(.*)' in json format on datasource (.*)")]
		public void WhenIInputAnManageAdherenceConfigurationScenarioForInJsonFormatOnDatasource(string personName, int datasource)
		{
			var personId = DataMaker.Person(personName).Person.Id.Value;
			var configuration = new
			{
				PlatformTypeId = Guid.Empty,
				SourceId = datasource,
				Persons = new[]
				{
					new
					{
						ExternalLogOn = personName,
						PersonId = personId
					}
				},
				States = new[] { "Phone" },
				ExpectedUpdates = 1,
				Timestamp = CurrentTime.Value()
			};

			var value = JsonConvert.SerializeObject(configuration, Formatting.Indented);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".scenario-configuration", value);
		}

		[When(@"I input a configuration for (.*) states and (.*) of (.*) persons can poll per second on datasource (.*)")]
		public void WhenIInputAConfigurationForEveryoneAndOfPersonsCanPollPerSecond(int stateCount, Decimal percent, int personCount, int datasource)
		{
			var configuration = new
			{
				PlatformTypeId = Guid.Empty,
				SourceId = datasource,
				Persons = new List<object>(),
				States = new List<object>(),
				Timestamp = CurrentTime.Value(),
				PollPerSecond = percent*personCount
			};

			for (var i = 0; i < stateCount; i++)
			{
				var state = "State" + i;
				configuration.States.Add(state);
			}
			for (var i = 0; i < personCount; i++)
			{
				var personName = "Person" + i;
				var personId = DataMaker.Person(personName).Person.Id.Value;
				configuration.Persons.Add(new { ExternalLogOn = personName, PersonId = personId});
			}
			
			var value = JsonConvert.SerializeObject(configuration, Formatting.Indented);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".scenario-configuration", value);
		}



		[Then(@"I should see a count of (.*) messages received for '(.*)'")]
		public void ThenIShouldSeeACountOfMessagesReceivedForEachApplicableModelUpdated(int messages, string model)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".message-count:contains('{0}') .message-target:contains('{1}')", model, messages);
			Browser.Interactions.AssertExistsUsingJQuery(".message-count:contains('{0}') .message-successes:contains('{1}')", model, messages);
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