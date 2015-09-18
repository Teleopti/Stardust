using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
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
			Http.PostJson(
				"Rta/State/Change",
				new ExternalUserStateWebModel
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
				Http.PostJson(
					"Rta/ActivityChange/CheckFor",
					new CheckForActivityChangeWebModel
					{
						PersonId = p.Person.Id.ToString(),
						BusinessUnitId = DefaultBusinessUnit.BusinessUnitFromFakeState.Id.ToString(),
						Tenant = UserConfigurable.DefaultTenantName
					});
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