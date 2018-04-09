using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Web.IntegrationTest.Areas.Stardust
{
	[StardustTest]
	public class Stardust
	{
		
		[Ignore("WIP"),Test]
		public void StardustEx()
		{
			startServiceBus();
		}

		private void startServiceBus()
		{
			var configReader = new TestConfigReader();
			configReader.ConfigValues.Add("ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/");
			configReader.ConfigValues.Add("NumberOfNodes", "1");
			
			var host = new ServiceBusRunner(i => { }, configReader);
			host.Start();
			//the test will stop in 10 sec
			Thread.Sleep(60000);
		}
	}

	public class TestConfigReader : ConfigReader
	{
		public readonly Dictionary<string, string> ConfigValues = new Dictionary<string, string>();
		
		public override string AppConfig(string name)
		{
			ConfigValues.TryGetValue(name, out var value);
			return value ?? base.AppConfig(name);
		}

		public override string ConnectionString(string name)
		{
			throw new System.NotImplementedException();
		}
	}
}