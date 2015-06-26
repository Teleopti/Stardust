using System.Collections.Specialized;
using System.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
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