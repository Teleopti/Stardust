using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
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

			var data = JsonConvert.SerializeObject(new ExternalUserStateWebModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				UserCode = personName,
				StateCode = stateCode,
				IsLoggedOn = true,
				Timestamp = CurrentTime.Value().ToString("yyyy-MM-dd HH:mm:ss"),
				PlatformTypeId = Guid.Empty.ToString(),
				SourceId = datasource.ToString(CultureInfo.InvariantCulture),
				BatchId = CurrentTime.Value().ToString("yyyy-MM-dd HH:mm:ss"),
				IsSnapshot = false
			});

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					var post = client.PostAsync(uri, new StringContent(data, Encoding.UTF8, "application/json"));
					var result = post.Result;
					if (result.StatusCode != HttpStatusCode.OK)
						throw new Exception("Posting rta state returned http code " + result.StatusCode);
				}
			}
		}

		[Given(@"there is a datasouce with id (.*)")]
		public void GivenThereIsADatasouceWithId(int datasourceId)
		{
			var datasource = new Datasources(datasourceId, " ", -1, " ", -1, " ", " ", 1, false, "6", false);
			DataMaker.Analytics().Apply(datasource);
		}
	}
}