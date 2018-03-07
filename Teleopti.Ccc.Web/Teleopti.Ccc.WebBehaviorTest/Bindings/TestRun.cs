using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class TestRun
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TestRun));
		private ITestInfo _testInfo;

		public void OneTimeSetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			log.Debug("Preparing for test run");

			Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(25));
			TestSiteConfigurationSetup.Setup();
			TestDataSetup.Setup();

			log.Debug("Starting test run");
		}

		public ITestInfo TestInfo() => _testInfo;

		public void BeforeTest(ITestInfo testInfo)
		{
			_testInfo = testInfo;

			log.Debug($"Preparing for test {TestInfo().Name()}");

			Browser.SelectBrowserByTag();

			ignoreTestIfDisabledByToggle();

			CurrentTime.Reset();
			CurrentScopeBusinessUnit.Reset();
			TestControllerMethods.BeforeTest();

			TestDataSetup.RestoreCcc7Data();
			TestDataSetup.ClearAnalyticsData();
			TestDataSetup.SetupDefaultScenario();
			TestDataSetup.CreateFirstTenantAdminUser();

			TestControllerMethods.ClearConnections();

			GlobalPrincipalState.EnsureThreadPrincipal();

			Browser.Interactions.Javascript("sessionStorage.clear();");

			log.Debug($"Starting test {TestInfo().Name()}");
		}

		public void AfterTest()
		{
			log.Debug($"Cleaning up after test {TestInfo().Name()}");
			log.Info(TestContext.CurrentContext.Result.Outcome.Status);
			if (Browser.IsStarted)
				Browser.Interactions.GoTo("about:blank");
			DataMaker.AfterTest();

			// some scenarios should not trigger handfire queue jobs which may throw an exception caused by no data available.
			if (!TestInfo().IsTaggedWith("suppressHangfireQueue"))
				LocalSystem.Hangfire.WaitForQueue();

			if (TestInfo().Error() != null)
				Console.WriteLine($@"Test {TestInfo().Name()} failed, please check the error message.");

			log.Debug($"Finished test {TestInfo().Name()}");
		}

		public void AfterStep()
		{
			if (TestInfo().Error() == null)
				return;
			log.Error("Step exception occurred, dumping info here.");
			Browser.Interactions.DumpInfo(log.Error);
		}

		public void OneTimeTearDown()
		{
			log.Debug("Cleaing up after test run");

			Browser.Dispose();
			TestSiteConfigurationSetup.TearDown();

			log.Debug("Finished test run");
		}


		private void ignoreTestIfDisabledByToggle()
		{
			var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");

			var runIfEnabled = TestInfo().Tags()
				.Where(s => s.StartsWith("OnlyRunIfEnabled"))
				.Select(onlyRunIfEnabled => (Toggles) Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
			var runIfDisabled = TestInfo().Tags()
				.Where(s => s.StartsWith("OnlyRunIfDisabled"))
				.Select(onlyRunIfDisabled => (Toggles) Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

			runIfEnabled.ForEach(t =>
			{
				if (!LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore($"Ignoring test {TestInfo().Name()} because toggle {t} is disabled");
			});

			runIfDisabled.ForEach(t =>
			{
				if (LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore($"Ignoring test {TestInfo().Name()} because toggle {t} is enabled");
			});
		}
	}
}