using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PhoneStateStepDefinitions
	{
		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public static int DataSourceId = 9;

		// this is the switch id
		// and is sent with the state
		public static string SourceId = "8"; 

		[Given(@"there is a switch")]
		public void GivenThereIsADatasouce()
		{
			// this maps the analytics data source id with the switch id
			var datasource = new Datasources(DataSourceId, " ", -1, " ", -1, " ", " ", 1, false, SourceId, false);
			DataMaker.Analytics().Apply(datasource);
		}

		[When(@"at '(.*)' '(.*)' sets (?:his|her) phone state to '(.*)'")]
		[Given(@"at '(.*)' '(.*)' sets (?:his|her) phone state to '(.*)'")]
		public void GivenAtSetsHisPhoneStateTo(string time, string personName, string stateCode)
		{
			CurrentTime.Set(time);
			WhenSetsHisPhoneStateToOnDatasource(personName, stateCode);
		}

		[When(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		[Given(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		public void WhenSetsHisPhoneStateToOnDatasource(string personName, string stateCode)
		{
			using (var h = new Http())
				h.PostJson(
					"Rta/State/Change",
					new ExternalUserStateWebModel
					{
						AuthenticationKey = "!#¤atAbgT%",
						UserCode = personName,
						StateCode = stateCode,
						SourceId = SourceId
					});
			LocalSystem.StateQueue.WaitForQueue();
			LocalSystem.Hangfire.WaitForQueue();
		}

	}
}