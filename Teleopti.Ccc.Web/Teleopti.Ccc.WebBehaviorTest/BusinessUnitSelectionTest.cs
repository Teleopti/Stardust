using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;
using SiteConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.SiteConfigurable;
using TeamConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.TeamConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[TestFixture]
	[FullIntegrationTest]
	public class BusinessUnitSelectionTest
	{
		[Test, Ignore("Too shaky, but the other test should cover it")]
		public void ShouldDisplayPermissionRolesForSelectedBusinessUnit()
		{
			var role = new RoleConfigurable
			{
				AccessToEverything = true,
				AccessToEveryone = true
			};
			DataMaker.Data().Apply(role);
			DataMaker.Data().Apply(new RoleForUser {Name = role.Name});
			DataMaker.Data().Apply(new BusinessUnitConfigurable {Name = "business unit 1"});
			DataMaker.Data().Apply(new BusinessUnitConfigurable {Name = "business unit 2"});
			DataMaker.Data().Apply(new RoleConfigurable {Name = "role 1", Description = "role 1", BusinessUnit = "business unit 1"});
			DataMaker.Data().Apply(new RoleConfigurable {Name = "role 2", Description = "role 2", BusinessUnit = "business unit 2"});

			TestControllerMethods.Logon();
			Navigation.GotoPermissions();

			Browser.Interactions.AssertNoContains(".wfm-list", ".wfm-list li", "role 1");
			Browser.Interactions.AssertNoContains(".wfm-list", ".wfm-list li", "role 2");

			Browser.Interactions.Click("[data-test-bu-select]");
			Browser.Interactions.ClickUsingJQuery("[data-test-bu-list] > li:contains(business unit 1)");
			Browser.Interactions.AssertAnyContains(".wfm-list li", "role 1");

			Browser.Interactions.Click("[data-test-bu-select]");
			Browser.Interactions.ClickUsingJQuery("[data-test-bu-list] > li:contains(business unit 2)");
			Browser.Interactions.AssertAnyContains(".wfm-list li", "role 2");
		}

		[Test]
		public void ShouldDisplayRtaOrganizationForSelectedBusinessUnit()
		{
			DataMaker.Data().Apply(new RoleConfigurable {Name = "full access", AccessToEverything = true, AccessToEveryone = true});
			DataMaker.Data().Apply(new RoleForUser {Name = "full access"});
			DataMaker.Data().Apply(new BusinessUnitConfigurable {Name = "business unit 1"});
			DataMaker.Data().Apply(new BusinessUnitConfigurable {Name = "business unit 2"});
			DataMaker.Data().Apply(new SiteConfigurable {Name = "site 1", BusinessUnit = "business unit 1"});
			DataMaker.Data().Apply(new TeamConfigurable {Name = "team 1", Site = "site 1"});
			DataMaker.Data().Apply(new SiteConfigurable {Name = "site 2", BusinessUnit = "business unit 2"});
			DataMaker.Data().Apply(new TeamConfigurable {Name = "team 2", Site = "site 2"});
			DataMaker.Data().Person("person 1").Apply(new PersonPeriodConfigurable {Team = "team 1", StartDate = "2014-01-21".Utc()});
			DataMaker.Data().Person("person 2").Apply(new PersonPeriodConfigurable {Team = "team 2", StartDate = "2014-01-21".Utc()});

			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAgents();

			Browser.Interactions.Click("[data-test-bu-select]");
			Browser.Interactions.ClickUsingJQuery("[data-test-bu-list] > li:contains(business unit 1)");
			Browser.Interactions.Click(".organization-picker");
			Browser.Interactions.AssertAnyContains(".organization-picker .md-label", "site 1");
			Browser.Interactions.AssertNoContains(".organization-picker", ".md-label", "site 2");
			Browser.Interactions.Click(".outside-click-backdrop");

			Browser.Interactions.Click("[data-test-bu-select]");
			Browser.Interactions.ClickUsingJQuery("[data-test-bu-list] > li:contains(business unit 2)");
			Browser.Interactions.Click(".organization-picker");
			Browser.Interactions.AssertAnyContains(".organization-picker .md-label", "site 2");
			Browser.Interactions.AssertNoContains(".organization-picker", ".md-label", "site 1");
			Browser.Interactions.Click(".outside-click-backdrop");
		}
	}
}