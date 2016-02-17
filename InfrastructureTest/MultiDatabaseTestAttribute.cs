namespace Teleopti.Ccc.InfrastructureTest
{
	public class MultiDatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();

			SetupFixtureForAssembly.RestoreCcc7Database();
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}

	}
}