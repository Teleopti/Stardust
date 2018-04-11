using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	public class TestConfigReader : ConfigReader
	{
		public readonly Dictionary<string, string> ConfigValues = new Dictionary<string, string>();
		
		public override string AppConfig(string name)
		{
			ConfigValues.TryGetValue(name, out var value);
			return value ?? base.AppConfig(name);
		}

		
	}
}