using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Bindings;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		public static TestRun TestRun;

		[OneTimeSetUp]
		public void Setup()
		{
			TestRun = new TestRun();
			TestRun.OneTimeSetUp();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			TestRun.OneTimeTearDown();
		}
	}
}