using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfigReader : IConfigReader
	{
		public FakeConfigReader(IEnumerable<KeyValuePair<string, string>> config)
		{
			AppSettings_DontUse = new NameValueCollection();
			config.ForEach(x => AppSettings_DontUse[x.Key] = x.Value);
			ConnectionStrings_DontUse = new ConnectionStringSettingsCollection();
		}

		public FakeConfigReader()
		{
			AppSettings_DontUse = new NameValueCollection();
			ConnectionStrings_DontUse = new ConnectionStringSettingsCollection();
		}

		public FakeConfigReader(string name, string value)
		{
			AppSettings_DontUse = new NameValueCollection();
			AppSettings_DontUse[name] = value;
			ConnectionStrings_DontUse = new ConnectionStringSettingsCollection();
		}

		public string AppConfig(string name)
		{
			return AppSettings_DontUse[name];
		}

		public NameValueCollection AppSettings_DontUse { get; set; }

		public ConnectionStringSettingsCollection ConnectionStrings_DontUse { get; set; }
	}
}