using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;


namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MyReportStepDefinition
	{
		[Then(@"MyReport tab should be visible")]
		public void ThenMyReportTabShouldBeVisible()
		{
			Browser.Interactions.AssertExists("a[href$='#MyReportTab']");
		}

		[Then(@"I should see a message saying I dont have access")]
		public void ThenIShouldSeeAMessageSayingIDontHaveAccess()
		{
			Browser.Interactions.AssertExists("#error-panel");
		}

		[Given(@"I do not have any report data for date '(.*)'")]
		public void GivenIDoNotHaveAnyReportDataForDate(DateTime date)
		{
			//don't persist anything
		}

		[Given(@"I have my report data for '(.*)'")]
		[Given(@"I have adherence report data for '(.*)'")]
		public void GivenIHaveMyReportDataFor(DateTime date)
		{
			var dateRow = DefaultAnalyticsDataCreator.GetDateRow(date);
			var dateId = DefaultAnalyticsDataCreator.GetDateId(date);
			var theDay = new SpecificDate { Date = new DateOnly(date), DateId = dateId, Rows = new[] { dateRow } };

			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();
			var timeZones = DefaultAnalyticsDataCreator.GetTimeZones();

			var personId = Person.FindPersonIdByPersonCode(DataMaker.Me().Person.Id.GetValueOrDefault());
			if (personId == -1)
			{
				var agent = new Person(DataMaker.Me().Person, datasourceData, 76, new DateTime(2010, 1, 1),
							 new DateTime(2059, 12, 31), 0, -2, 0, DefaultBusinessUnit.BusinessUnit.Id.Value, false, timeZones.CetTimeZoneId, Guid.NewGuid());
				DataMaker.Data().Analytics().Setup(agent);
				personId = agent.PersonId;
			};

			const int acdLoginId = 123;
			const int scenarioId = 12;

			//common analytics data
			DataMaker.Data().Analytics().Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));

			//some report data
			const int intervalId = 32;
			var queues = new AQueue(datasourceData) {QueueId = 5};

			DataMaker.Analytics().Setup(queues);
			
			DataMaker.Data().Analytics().Setup(new FillBridgeTimeZoneFromData(theDay, DefaultAnalyticsDataCreator.GetInterval(), timeZones, datasourceData));
			DataMaker.Data().Analytics().Setup(new FactSchedule(personId, dateId, dateId, 0, 22, intervalId, scenarioId));
			DataMaker.Data().Analytics().Setup(new FactAgent(dateId, intervalId, acdLoginId, 600, 900, 300, 55,0, 0, 7, 210, 60));
            DataMaker.Data().Analytics().Setup(new FactAgentQueue(dateId, intervalId, queues.QueueId, acdLoginId, 210, 60, 7, 0));
			DataMaker.Data().Analytics().Setup(new FactScheduleDeviation(dateId, dateId, intervalId, personId, 900, 60, 60, 60, true));
		}

		[Given(@"I have adherence report data for mobile view for '(.*)'")]
		public void GivenIHaveAdherenceReportDataForMobileViewFor(DateTime date)
		{
			var dateId = DefaultAnalyticsDataCreator.GetDateId(date);
			var dateRow = DefaultAnalyticsDataCreator.GetDateRow(date);
			var timeZones = DefaultAnalyticsDataCreator.GetTimeZones();
			var theDay = new SpecificDate { Date = new DateOnly(date), DateId = dateId, Rows = new[] { dateRow } };
			var intervals = DefaultAnalyticsDataCreator.GetInterval();
			var datasource = DefaultAnalyticsDataCreator.GetDataSources();

			var personId = Person.FindPersonIdByPersonCode(DataMaker.Me().Person.Id.GetValueOrDefault());
			if (personId == -1)
			{
				var agent = new Person(DataMaker.Me().Person, datasource, 76, new DateTime(2010, 1, 1),
							 new DateTime(2059, 12, 31), 0, -2, 0, DefaultBusinessUnit.BusinessUnit.Id.Value, false, timeZones.CetTimeZoneId);
				DataMaker.Data().Analytics().Setup(agent);
				personId = agent.PersonId;
			};
			const int acdLoginId = 123;
			const int scenarioId = 12;


			//common analytics data
			DataMaker.Data().Analytics().Setup(new FillBridgeTimeZoneFromData(theDay, intervals, timeZones, datasource));
			DataMaker.Data().Analytics().Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));

			//some report data
			var intervalId = 2;
			setReportData(personId, theDay, intervalId, scenarioId, acdLoginId);

			intervalId = 80;
			setReportData(personId, theDay, intervalId, scenarioId, acdLoginId);
		
		}

		private static void setReportData(int personId, SpecificDate theDay, int intervalId, int scenarioId, int acdLoginId)
		{
			DataMaker.Data()
				.Analytics()
				.Setup(new FactSchedule(personId, theDay.DateId, theDay.DateId, 0, 22, intervalId, scenarioId));
			DataMaker.Data()
				.Analytics()
				.Setup(new FactAgent(theDay.DateId, intervalId, acdLoginId, 600, 900, 300, 55, 0, 0, 7,
					210, 60));
			DataMaker.Data().Analytics().Setup(new FactAgentQueue(theDay.DateId, intervalId, 5, acdLoginId, 210, 60, 7, 0));
			DataMaker.Data()
				.Analytics()
				.Setup(new FactScheduleDeviation(theDay.DateId, theDay.DateId, intervalId, personId, 900, 60, 60, 60, true));
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

		[Then(@"I should see the detailed adherence report for '(.*)'")]
		public void ThenIShouldSeeTheDetailedAdherenceReportFor(DateTime date)
		{
			Browser.Interactions.AssertExists(".vertical-progress-bar");
			Browser.Interactions.AssertExists(".schedule-interval");
		}


		[When(@"I navigate to the previous day")]
        public void WhenINavigateToThePreviousDay()
        {
            Browser.Interactions.Click("#report-view-date-nav-prev");
        }

		[When(@"I navigate to the next day")]
		public void WhenINavigateToTheNextDay()
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
			Browser.Interactions.ClickUsingJQuery(".glyphicon.glyphicon-calendar");
			string selector = string.Format(".datepicker-days .day:contains('{0}')", date.Day);
			Browser.Interactions.ClickUsingJQuery(selector);
		}

		[When(@"I choose to view my detailed adherence")]
		public void WhenIChooseToViewMyDetailedAdherence()
		{
			Browser.Interactions.Click(".adherenceValue");
		}

		[When(@"I choose to go to overview")]
		public void WhenIChooseToGoToOverview()
		{
			Browser.Interactions.Click("#report-overview");
		}

		[Then(@"I should end up in my adherence report for '(.*)'")]
		public void ThenIShouldEndUpInMyAdherenceReportFor(DateTime date)
		{
			Browser.Interactions.AssertUrlContains(string.Format("Adherence/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
		}

        [Then(@"I should see the queue metrics report for '(.*)'")]
        public void ThenIShouldSeeTheQueueMetricsReportFor(DateTime date)
        {
            Browser.Interactions.AssertUrlContains(string.Format("QueueMetrics/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
        }

		[Then(@"I should see the queue metrics report with data for '(.*)'")]
		public void ThenIShouldSeeTheQueueMetricsReportWithDataFor(string p0)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.queue-handling-time').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.queue-talk-time').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.queue-afterwork-time').text().length > 0;", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.queue-answered').text().length > 0;", "True");
		}

		[When(@"I choose to view my queue metrics")]
		public void WhenIChooseToViewMyQueueMetrics()
		{
			Browser.Interactions.Click(".queue-metrics-value");
		}

	}
}
