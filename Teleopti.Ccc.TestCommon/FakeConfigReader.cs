using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MultipleConfig;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfigReader : IConfigReader
	{
		public FakeConfigReader(IEnumerable<KeyValuePair<string, string>> config)
		{
			AppSettings = new NameValueCollection();
			config.ForEach(x => AppSettings[x.Key] = x.Value);
			ConnectionStrings = new ConnectionStringSettingsCollection();
		}

		public FakeConfigReader()
		{
			AppSettings = new NameValueCollection();
			ConnectionStrings = new ConnectionStringSettingsCollection();
		}

		public string AppConfig(string name)
		{
			return AppSettings[name];
		}

		public NameValueCollection AppSettings { get; set; }

		public ConnectionStringSettingsCollection ConnectionStrings { get; set; }
	}
}