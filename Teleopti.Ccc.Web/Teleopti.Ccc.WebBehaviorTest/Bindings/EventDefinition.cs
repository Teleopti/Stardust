using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class EventDefinition
	{
		[AfterScenario]
		public void AfterScenario()
		{
			if (!Browser.IsStarted()) return;
			try
			{
				if (ScenarioContext.Current.TestError != null)
				{
					var artifactFileName = GetCommonArtifactFileNameForScenario();
					SaveScreenshot(artifactFileName);
					throw MakeScenarioException(artifactFileName);
				}
			}
			finally
			{
				Browser.ForciblyClose();
			}
		}

		private static ScenarioErrorException MakeScenarioException(string artifact)
		{
			var html = Browser.Current.Html;
			var text = Browser.Current.Text;
			var message = string.Format("Scenario error occurred.\nArtifact:{0}\n\nText:\n{1}\n\nHtml:\n{2}", artifact, text, html);
			return new ScenarioErrorException(message, ScenarioContext.Current.TestError);
		}

		private string GetCommonArtifactFileNameForScenario()
		{
			var fileName = ScenarioContext.Current.ScenarioInfo.Title + "." + Path.GetRandomFileName();
			fileName = new string(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
			return Path.Combine(Environment.CurrentDirectory, fileName);
		}

		private static void SaveScreenshot(string artifactFileName)
		{
			Browser.Current.CaptureWebPageToFile(artifactFileName + ".jpg");
		}

		[BeforeTestRun]
		public static void BeforeTestRun()
		{
			log4net.Config.XmlConfigurator.Configure();
			TestDataSetup.Setup();
			TestSiteConfigurationSetup.Setup();
			Browser.MakeSureBrowserIsNotRunning();
			EventualTimeouts.Set(TimeSpan.FromSeconds(5));
		}

		[AfterTestRun]
		public static void AfterTestRun() { TestSiteConfigurationSetup.TearDown(); }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class ScenarioErrorException : Exception
	{
		public ScenarioErrorException(string message, Exception scenarioError) : base(message, scenarioError) {  }
	}
}