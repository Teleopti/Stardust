using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Toggle;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class TestRun
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TestRun));
		private const string suppressHangfireQueueTag = "suppressHangfireQueue";

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

		public void BeforeTest()
		{
			log.Debug($"Preparing for test {ScenarioContext.Current?.ScenarioInfo.Title}");

			Browser.SelectBrowserByTag();

			ignoreScenarioIfDisabledByToggle();

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

			log.Debug($"Starting test {ScenarioContext.Current?.ScenarioInfo.Title}");
		}

		public void AfterTest()
		{
			log.Debug($"Cleaning up after test {ScenarioContext.Current?.ScenarioInfo.Title}");
			log.Info(TestContext.CurrentContext.Result.Outcome.Status);
			if (Browser.IsStarted)
				Browser.Interactions.GoTo("about:blank");
			DataMaker.AfterTest();
			// some scenarios should not trigger handfire queue jobs which may throw an exception caused by no data available.
			if (!suppressHangfireQueue())
			{
				LocalSystem.Hangfire.WaitForQueue();
			}

			if (ScenarioContext.Current?.TestError != null)
				Console.WriteLine($@"Test {ScenarioContext.Current?.ScenarioInfo.Title} failed, please check the error message.");

			log.Debug($"Finished test {ScenarioContext.Current?.ScenarioInfo.Title}");
		}

		public void AfterStep()
		{
			if (ScenarioContext.Current.TestError == null) return;
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

		private static bool suppressHangfireQueue()
		{
			return ScenarioContext.Current?.IsTaggedWith(suppressHangfireQueueTag) ?? false;
		}


		private static void ignoreScenarioIfDisabledByToggle()
		{
			if (ScenarioContext.Current == null)
				return;

			var matchingEnkelsnuffs = new Regex(@"\'(.*)\'");

			var tags = ScenarioContext.Current.ScenarioInfo.Tags.Union(FeatureContext.Current.FeatureInfo.Tags).ToArray();
			var runIfEnabled = tags.Where(s => s.StartsWith("OnlyRunIfEnabled"))
				.Select(onlyRunIfEnabled => (Toggles) Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfEnabled).Groups[1].ToString()));
			var runIfDisabled = tags.Where(s => s.StartsWith("OnlyRunIfDisabled"))
				.Select(onlyRunIfDisabled => (Toggles) Enum.Parse(typeof(Toggles), matchingEnkelsnuffs.Match(onlyRunIfDisabled).Groups[1].ToString()));

			runIfEnabled.ForEach(t =>
			{
				if (!LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore($"Ignoring test {ScenarioContext.Current.ScenarioInfo.Title} because toggle {t} is disabled");
			});

			runIfDisabled.ForEach(t =>
			{
				if (LocalSystem.Toggles.IsEnabled(t))
					Assert.Ignore($"Ignoring test {ScenarioContext.Current.ScenarioInfo.Title} because toggle {t} is enabled");
			});
		}
	}
}