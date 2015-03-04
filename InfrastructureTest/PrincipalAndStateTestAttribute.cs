namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			SetupFixtureForAssembly.BeforeTest();
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.AfterTest();
		}
	}
}