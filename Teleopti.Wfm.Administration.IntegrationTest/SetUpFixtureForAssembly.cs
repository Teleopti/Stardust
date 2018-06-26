using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		internal static IPerson loggedOnPerson;
		internal static IApplicationData ApplicationData;
		internal static IDataSource DataSource;
		private static int dataHash = 0;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

	}

}
