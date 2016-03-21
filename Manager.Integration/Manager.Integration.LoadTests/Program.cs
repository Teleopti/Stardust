using System.IO;
using System.Reflection;
using Manager.Integration.Test.LoadTests;
using NUnit.Core;
using NUnit.Core.Filters;

namespace Manager.Integration.LoadTests
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			//RunTestVersion1();

			RunTestVersion2();
		}

		private static void RunTestVersion2()
		{
			// Run Nunit test programmatically.

			CoreExtensions.Host.InitializeService();

			var fileInfo =
				new FileInfo(Assembly.GetExecutingAssembly().Location);


			var testPackage =
				new TestPackage(Path.Combine(fileInfo.Directory.FullName,
				                             "Manager.Integration.Test.dll"));

			var remoteTestRunner = new RemoteTestRunner();

			remoteTestRunner.Load(testPackage);

			var testName = 
				"Manager.Integration.Test.FunctionalTests.OneManagerAndOneNodeTests.ShouldBeAbleToCreateASuccessJobRequestTest";

			var simpleNameFilter =
				new SimpleNameFilter(testName);

			var testResult =
				remoteTestRunner.Run(new NullListener(),
				                     simpleNameFilter,
				                     false,
				                     LoggingThreshold.Error);
		}

		private static void RunTestVersion1()
		{
			//------------------------------------
			// Do NOT change order of execution.
			//------------------------------------
			var loadTests = new OneManagerAndOneNodeLoadTests();

			// Set up test.
			loadTests.TestFixtureSetUp();
			loadTests.SetUp();

			// Execute test.
			loadTests.YourTestGoesHere();

			// Tear down test.
			loadTests.TearDown();
			loadTests.TestFixtureTearDown();
		}
	}
}