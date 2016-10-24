using System.Linq;
using Autofac;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BusinessUnitStepDefinitions
	{
		

		[Given(@"there is a business unit named '(.*)'")]
		public void GivenThereIsABusinessUnitWith(string name)
		{
			var businessUnitApp = new BusinessUnitConfigurable { Name = name };
			DataMaker.Data().Apply(businessUnitApp);

			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			if (!toggleQuerier.IsEnabled(Toggles.ETL_SpeedUpIntradayBusinessUnit_38932))
			{
				var businessUnitAnalytics = new BusinessUnit(businessUnitApp.BusinessUnit, DefaultAnalyticsDataCreator.GetDataSources(), ++BusinessUnit.IdCounter);
				DataMaker.Data().Analytics().Apply(businessUnitAnalytics);
			}
		}

		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnitApp = table.CreateInstance<BusinessUnitConfigurable>();
			DataMaker.Data().Apply(businessUnitApp);

			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			if (!toggleQuerier.IsEnabled(Toggles.ETL_SpeedUpIntradayBusinessUnit_38932))
			{
				var businessUnitAnalytics = new BusinessUnit(businessUnitApp.BusinessUnit, DefaultAnalyticsDataCreator.GetDataSources(), ++BusinessUnit.IdCounter);
				DataMaker.Data().Analytics().Apply(businessUnitAnalytics);
			}
		}

		[Given(@"the business unit scope is using '(.*)'")]
		public void GivenTheBusinessUnitScopeIsUsing(string name)
		{
			var businessUnitConfigurable = DataMaker.Data().UserDatasOfType<BusinessUnitConfigurable>().First(x => x.Name == name);
			var businessUnitScope = IntegrationIoCTest.Container.Resolve<IBusinessUnitScope>();
			
			businessUnitScope.OnThisThreadUse(businessUnitConfigurable.BusinessUnit);
		}

		[Given(@"the business unit scope is reset")]
		public void GivenTheBusinessUnitScopeIsReset()
		{
			var businessUnitScope = IntegrationIoCTest.Container.Resolve<IBusinessUnitScope>();
			businessUnitScope.OnThisThreadUse(null);
		}
	}
}