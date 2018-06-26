using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}
	}
}