using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest
{
	public class FullIntegrationTestAttribute : IntegrationIoCTestAttribute
	{
		protected override void BeforeTest()
		{
			base.BeforeTest();
			SetupFixtureForAssembly.TestRun.BeforeTest();

			var role = new RoleConfigurable
			{
				AccessToEverything = true,
				AccessToEveryone = true
			};
			DataMaker.Data().Apply(role);
			DataMaker.Data().Apply(new RoleForUser {Name = role.Name});

			TestControllerMethods.Logon();
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.TestRun.AfterTest();
			base.AfterTest();
		}
	}

	[TestFixture]
	[FullIntegrationTest]
	public class UpgradeTest
	{
		[Test]
		[OnlyRunIfEnabled(Toggles.RTA_ReloadUIOnSystemVersionChanged_48196)]
		public void ShouldRefreshClientApplicationAfterUpgrade()
		{
			Navigation.GoToPage($"wfm/#/teapot");
			Browser.Interactions.AssertFirstContains("body", "Bad coffee");

			TestControllerMethods.SetVersion("2.0");

			Browser.Interactions.AssertFirstContains("body", "I'm a teapot");
		}
	}
}