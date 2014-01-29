using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MyReportStepDefinition
	{
		[Then(@"MyReport tab should not be visible")]
		public void ThenReportTabShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotExists(".navbar-inner","a[href$='#MyReportTab']");
		}

		[Then(@"MyReport tab should be visible")]
		public void ThenMyReportTabShouldBeVisible()
		{
			Browser.Interactions.AssertExists("a[href$='#MyReportTab']");
		}

		[Then(@"I should see MyReport for '(.*)'")]
		public void ThenIShouldSeeMyReportFor(string date)
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see a message saying I dont have access to MyReport")]
		public void ThenIShouldSeeAMessageSayingIDontHaveAccess()
		{
			//todo...
			Browser.Interactions.AssertExists(".error");
		}

		[Given(@"I answered '(.*)' calls on the date '(.*)'")]
		public void GivenIAnsweredCallsOnTheDate(string calls, DateTime date)
		{
			DataMaker.Data().Apply(new PreferenceConfigurable { Date = date, IsExtended = true });
		
		}

		[Given(@"I do not have any report data for date '(.*)'")]
		public void GivenIDoNotHaveAnyReportDataForDate(DateTime date)
		{
			//don't persist anything
		}

		[Given(@"I have my report data for '(.*)'")]
		public void GivenIHaveMyReportDataFor(DateTime date)
		{
			var timeZones = new UtcAndCetTimeZones();
			var today = new SpecificDate { Date = new DateOnly(date) };
			var intervals = new QuarterOfAnHourInterval();
			var datasource = new ExistingDatasources(timeZones);

			const int personId = 76;
			const int acdLoginId = 123;
			const int scenarioId = 12;

			var agent = new Person(DataMaker.Me().Person, datasource, personId, new DateTime(2010, 1, 1),
						 new DateTime(2059, 12, 31), 0, -2, 0, TestData.BusinessUnit.Id.Value, false, timeZones.CetTimeZoneId);
			var scenario = Scenario.DefaultScenarioFor(scenarioId, TestData.BusinessUnit.Id.Value);

			//common analytics data
			DataMaker.Data().Analytics().Setup(new EternityAndNotDefinedDate());
			DataMaker.Data().Analytics().Setup(timeZones);
			DataMaker.Data().Analytics().Setup(today);
			DataMaker.Data().Analytics().Setup(intervals);
			DataMaker.Data().Analytics().Setup(datasource);
			DataMaker.Data().Analytics().Setup(new FillBridgeTimeZoneFromData(today, intervals, timeZones, datasource));
			DataMaker.Data().Analytics().Setup(agent);
			DataMaker.Data().Analytics().Setup(new FillBridgeAcdLoginPersonFromData(agent, acdLoginId));
			DataMaker.Data().Analytics().Setup(scenario);

			//some report data
			DataMaker.Data().Analytics().Setup(new FactSchedule(personId, today.DateId, 0, 22, 1, scenarioId));
		}

		[Then(@"I should see my report with data for '(.*)'")]
		public void ThenIShouldSeeMyReportWithDataFor(DateTime date)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.adherenceValue').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.readyTimePerScheduledReadyTimeValue').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.handlingTimeValue').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.talkTimeValue').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.afterWorkValue').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.answeredCallsValue').text().length > 0;", "True");
		}
	}
}
