using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class AssemblyInfoTest
	{
		[Test]
		public void GlobalAssemblyFileOnlyExistOnce()
		{
			var numberOfGlobalAssemblyInfos = solutionDirectory().EnumerateFiles("GlobalAssemblyInfo.cs", SearchOption.AllDirectories).Count();
			Assert.AreEqual(1, numberOfGlobalAssemblyInfos, "You should link to GlobalAssemblyInfo, not create a new one!");
		}

		[Test]
		public void AssemblyInfosShouldBeAvoided()
		{
			var exceptions = new[]
			{
				"PBI30532LoadTest", "Teleopti.Analytics.EtlServiceStarter", "SdkTestClientWin", "SdkTestWinGui",
				"Teleopti.Ccc.Sdk.LoadTestClient", "Teleopti.Ccc.Sdk.Samples", "TeleoptiControls",
				"Teleopti.Common.UI.OutlookControls", "Teleopti.Runtime.Environment", "Teleopti.Runtime.TestServer",
				"Teleopti.Support.PreReqsCheck", "Teleopti.Ccc.Web", "ReplaceString", "Teleopti.Ccc.WebBehaviorTest"
			};

			foreach (var assemblyInfo in solutionDirectory()
													.EnumerateFiles("AssemblyInfo.cs", SearchOption.AllDirectories)
													.Where(assemblyInfo => !exceptions.Any(ex => assemblyInfo.DirectoryName.Contains(ex))))
			{
				Assert.Fail("Don't add new {0} if really needed, add a link to [repo]/globalassemblyinfo.cs instead!", assemblyInfo.DirectoryName);
			}
		}


		private static DirectoryInfo solutionDirectory()
		{
			//hack -assumes always runs in infrastructuretest/bin/debug|release
			return Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
		}
	}
}