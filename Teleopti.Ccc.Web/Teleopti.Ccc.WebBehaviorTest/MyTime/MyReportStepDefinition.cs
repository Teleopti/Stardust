using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
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
            Browser.Interactions.AssertNotExists(".navbar-default", "a[href$='#MyReportTab']");
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
			var theDay = new SpecificDate { Date = new DateOnly(date) };
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
			DataMaker.Data().Analytics().Setup(theDay);
			DataMaker.Data().Analytics().Setup(intervals);
			DataMaker.Data().Analytics().Setup(datasource);
			DataMaker.Data().Analytics().Setup(new FillBridgeTimeZoneFromData(theDay, intervals, timeZones, datasource));
			DataMaker.Data().Analytics().Setup(agent);
			DataMaker.Data().Analytics().Setup(new FillBridgeAcdLoginPersonFromData(agent, acdLoginId));
			DataMaker.Data().Analytics().Setup(scenario);

			//some report data
			const int intervalId = 32;
			DataMaker.Data().Analytics().Setup(new FactSchedule(personId, theDay.DateId, theDay.DateId, 0, 22, intervalId, scenarioId));
			DataMaker.Data().Analytics().Setup(new FactAgent(theDay.DateId, intervalId, acdLoginId, theDay.DateId, intervalId - 8, 600, 900, 300, 55,0, 0, 7, 210, 60));
			DataMaker.Data().Analytics().Setup(new FactAgentQueue(theDay.DateId, intervalId, 5, acdLoginId, 210, 60, 7, 0));
			DataMaker.Data().Analytics().Setup(new FactScheduleDeviation(theDay.DateId, theDay.DateId, intervalId, personId, 900, 60, 60, 60, true));
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

        [When(@"I click previous button")]
        public void WhenIClickPreviousButton()
        {
            Browser.Interactions.Click("#report-view-date-nav-prev");
        }

        [When(@"I click next button")]
        public void WhenIClickNextButton()
        {
            Browser.Interactions.Click("#report-view-date-nav-next");
        }

		[Then(@"I should end up in my report for '(.*)'")]
		public void ThenIShouldEndUpInMyReportFor(DateTime date)
		{

			Browser.Interactions.AssertUrlContains(string.Format("Index/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
		}

		[When(@"I select the date '(.*)'")]
		public void WhenISelectTheDate(DateTime date)
		{
            Browser.Interactions.Click(".glyphicon.glyphicon-calendar");
			string selector = string.Format(".datepicker-days .day:contains('{0}')", date.Day);
			Browser.Interactions.AssertVisibleUsingJQuery(selector);
			Browser.Interactions.ClickUsingJQuery(selector);
		}


	}
}
