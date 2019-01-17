using NUnit.Framework;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.CommonTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			StateHolderProxyHelper.ClearStateHolder();
		}
        
    }
}