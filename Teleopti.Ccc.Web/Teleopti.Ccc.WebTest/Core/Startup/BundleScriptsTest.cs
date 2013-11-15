using System.Web.Optimization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	//can't really find a good way unit tests this without just repeating real code here...
	//realy on web behavior tests 

	[TestFixture]
	public class BundleScriptsTest
	{
		 [Test]
		 public void ShouldAddClientFiles()
		 {
			 BundleTable.Bundles.Clear();
			 new BundleScripts().Execute();
			 BundleTable.Bundles.Should().Not.Be.Empty();
		 }
	}
}