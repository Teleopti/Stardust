using System;
using System.Linq;
using Polly;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PhoneStateStepDefinitions
	{
		private readonly IRtaEventStoreTester _events;
		private readonly DataMakerImpl _data;

		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public static int DataSourceId = 9;

		// this is the switch id
		// and is sent with the state
		public static string SourceId = "8";

		public PhoneStateStepDefinitions(IRtaEventStoreTester events, DataMakerImpl data)
		{
			_events = events;
			_data = data;
		}

		[Given(@"there is a switch")]
		public void GivenThereIsADatasouce()
		{
			// this maps the analytics data source id with the switch id
			var datasource = new Datasources(DataSourceId, " ", -1, " ", -1, " ", " ", 1, false, SourceId, false);
			_data.Data().Analytics().Apply(datasource);
		}

		[When(@"at '(.*)' '(.*)' sets (?:his|her) phone state to '(.*)'")]
		[Given(@"at '(.*)' '(.*)' sets (?:his|her) phone state to '(.*)'")]
		public void GivenAtSetsHisPhoneStateTo(string time, string userCode, string stateCode)
		{
			CurrentTime.Set(time);
			WhenSetsHisPhoneStateToOnDatasource(userCode, stateCode);
		}

		[When(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		[Given(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		public void WhenSetsHisPhoneStateToOnDatasource(string userCode, string stateCode)
		{
			using (var h = new Http())
				h.PostJson(
					"Rta/State/Change",
					new ExternalUserStateWebModel
					{
						AuthenticationKey = "!#¤atAbgT%",
						UserCode = userCode,
						StateCode = stateCode,
						SourceId = SourceId
					});

			waitForStateProcessing(userCode, stateCode);
		}

		private void waitForStateProcessing(string userCode, string stateCode)
		{
			var personId = _data.Data().Person(userCode).Person.Id.GetValueOrDefault();
			var time = CurrentTime.Value();
			var stateChangeProcessed = Policy.HandleResult(false)
				.WaitAndRetry(50, attempt => TimeSpan.FromMilliseconds(100))
				.Execute(() =>
				{
					var matchingEvents = from e in _events.LoadAllForTest()
						let stateChanged = e as PersonStateChangedEvent
						where stateChanged != null &&
							  stateChanged.PersonId == personId &&
							  stateChanged.Timestamp == time &&
							  stateChanged.StateName == stateCode
						select stateChanged;

					return matchingEvents.Any();
				});
			if (!stateChangeProcessed)
				throw new WaitForStateProcessException($"State {userCode}/{stateCode} was not processed in time");
		}

		public class WaitForStateProcessException : Exception
		{
			public WaitForStateProcessException(string message) : base(message)
			{
			}
		}
	}
}