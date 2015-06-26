using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfigurationWrapper : IConfigurationWrapper
	{
		public IDictionary<string, string> AppSettings { get; set; }

		public FakeConfigurationWrapper()
		{
			AppSettings = new Dictionary<string, string>();
		}

		public FakeConfigurationWrapper(IDictionary<string, string> appSettings)
		{
			AppSettings = appSettings;
		}
	}
}