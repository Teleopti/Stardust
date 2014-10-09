using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PhoneStateStepDefinitions
	{
		[Given(@"there is an external logon named '(.*)' with datasource (.*)")]
		public void GivenThereIsAnExternalLogonNamedWithDatasource(string acdLogOnName, int datasourceId)
		{
			DataMaker.Data().Apply(new ExternalLogonConfigurable()
			{
				AcdLogOnName = acdLogOnName,
				DataSourceId = datasourceId,
				AcdLogOnOriginalId = acdLogOnName
			});
		}

		[When(@"'(.*)' sets (?:his|her) phone state to '(.*)' on datasource (.*)")]
		[Given(@"'(.*)' sets (?:his|her) phone state to '(.*)' on datasource (.*)")]
		public void WhenSetsHisPhoneStateToOnDatasource(string personName, string stateCode, int datasource)
		{
			var uri = new Uri(TestSiteConfigurationSetup.URL, "Rta/Service/SaveExternalUserState");

			var data = JsonConvert.SerializeObject(new AgentStateInputModel
			{
				AuthenticationKey = "!#�atAbgT%",
				UserCode = personName,
				StateCode = stateCode,
				IsLoggedOn = true,
				Timestamp = CurrentTime.Value().ToString("yyyy-MM-dd HH:mm:ss"),
				PlatformTypeId = Guid.Empty.ToString(),
				SourceId = datasource.ToString(CultureInfo.InvariantCulture),
				IsSnapshot = false
			});

			var request = (HttpWebRequest) WebRequest.Create(uri);
			request.Method = "POST";
			request.ContentType = "application/json";
			using (var writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(data);
			}
			// dont forget to close!
			request.GetResponse().Close();
		}

		[Given(@"there is a datasouce with id (.*)")]
		public void GivenThereIsADatasouceWithId(int datasourceId)
		{
			var datasource = new Datasources(datasourceId, " ", -1, " ", -1, " ", " ", 1, false, "6", false);
			DataMaker.Analytics().Apply(datasource);
		}
	}
}