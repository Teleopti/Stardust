using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class DatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();

			SetupFixtureForAssembly.RestoreCcc7Database();
		}
		
	}
}