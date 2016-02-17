namespace Teleopti.Ccc.InfrastructureTest
{
	public class AnalyticsDatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();

			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}

	}
}