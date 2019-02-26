using System.IO;
using NUnit.Framework;

namespace Teleopti.Wfm.Adherence.Test.Configuration
{
	[SetUpFixture]
	[Parallelizable(ParallelScope.Fixtures)]
	public class Parallelize
	{
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			// when parallelizing, using (testDirectoryFix()) doesnt work properly in DatabaseTestHelper
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}
	}
}