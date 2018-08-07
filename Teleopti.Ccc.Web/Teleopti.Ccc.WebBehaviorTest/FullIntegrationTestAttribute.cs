using NUnit.Framework.Interfaces;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest
{
	public class FullIntegrationTestAttribute : IntegrationTestAttribute
	{
		public override void BeforeTest(ITest testDetails)
		{
			base.BeforeTest(testDetails);
			
			SetupFixtureForAssembly.TestRun.BeforeTest(new NUnitTest(testDetails));

			var role = new RoleConfigurable
			{
				AccessToEverything = true,
				AccessToEveryone = true
			};
			DataMaker.Data().Apply(role);
			DataMaker.Data().Apply(new RoleForUser {Name = role.Name});

			TestControllerMethods.Logon();
		}

		public override void AfterTest(ITest testDetails)
		{
			SetupFixtureForAssembly.TestRun.AfterTest();
			base.AfterTest(testDetails);
		}
	}
}