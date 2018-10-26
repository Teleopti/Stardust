using System.IO;
using NUnit.Framework;


namespace Teleopti.Ccc.InfrastructureTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}
	}
}
