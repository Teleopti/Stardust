using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfiguratoinWrapper : IConfigurationWrapper
	{
		public IDictionary<string, string> AppSettings { get; set; }

		public FakeConfiguratoinWrapper()
		{
			AppSettings = new Dictionary<string, string>();
		}
	}

	public class FakeConfigReader : IConfigReader
	{
		public FakeConfigReader()
		{
			AppSettings = new NameValueCollection();
			ConnectionStrings = new ConnectionStringSettingsCollection();
		}

		public NameValueCollection AppSettings { get; set; }

		public ConnectionStringSettingsCollection ConnectionStrings { get; set; }
	}
}