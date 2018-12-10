using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class AppInsightsConfigTest
	{
		[Test]
		public void SholdHaveAtLeastOneAppInsightsConfigForSendingPerformanceData()
		{
			var appInsightsConfigFiles = solutionDirectory().EnumerateFiles("*ApplicationInsights.config", SearchOption.AllDirectories);
			foreach (var appInsightsConfigFile in appInsightsConfigFiles)
			{
				var doc = new XmlDocument();
				doc.Load(appInsightsConfigFile.FullName);

				var items = doc.DocumentElement?["TelemetryModules"]?.ChildNodes;
				if (items == null) continue;
				foreach (XmlNode item in items)
				{
					if (item.Attributes["Type"].Value ==
						"Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.PerformanceCollectorModule, Microsoft.AI.PerfCounterCollector"
					)
						return;
				}
			}

			Assert.Fail("At least one application insights configuration file must send perfmon data.");
		}

		private static DirectoryInfo solutionDirectory()
		{
			//hack -assumes always runs in infrastructuretest/bin/debug|release
			return Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
		}
	}
}