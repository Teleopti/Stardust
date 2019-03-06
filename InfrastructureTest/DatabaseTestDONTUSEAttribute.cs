using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class DatabaseTestDONTUSEAttribute : InfrastructureTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();

			SetupFixtureForAssembly.RestoreCcc7Database();
		}
	}
}