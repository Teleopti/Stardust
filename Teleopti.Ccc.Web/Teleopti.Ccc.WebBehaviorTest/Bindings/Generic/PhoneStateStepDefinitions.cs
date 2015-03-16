using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;

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
			Http.PostJson("Rta/State/Change", new ExternalUserStateWebModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				UserCode = personName,
				StateCode = stateCode,
				IsLoggedOn = true,
				PlatformTypeId = Guid.Empty.ToString(),
				SourceId = datasource.ToString(CultureInfo.InvariantCulture),
				BatchId = CurrentTime.Value().ToString("yyyy-MM-dd HH:mm:ss"),
				IsSnapshot = false
			});
		}

		public static void CheckForActivityChange()
		{
			DataMaker.Data().AllPersons().ForEach(p =>
			{
				var data = new CheckForActivityChangeWebModel
				{
					PersonId = p.Person.Id.ToString(),
					BusinessUnitId = DefaultBusinessUnit.BusinessUnitFromFakeState.Id.ToString()
				};
				Http.PostJson( "Rta/ActivityChange/CheckFor", data);
			});
		}

		[Given(@"there is a datasouce with id (.*)")]
		public void GivenThereIsADatasouceWithId(int datasourceId)
		{
			var datasource = new Datasources(datasourceId, " ", -1, " ", -1, " ", " ", 1, false, "6", false);
			DataMaker.Analytics().Apply(datasource);
		}
	}
}