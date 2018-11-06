using System.IO;
using NUnit.Framework;

namespace Teleopti.Wfm.Adherence.Test
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
