using System.Collections.Generic;
using Teleopti.Ccc.IocCommon.MultipleConfig;

namespace Teleopti.Ccc.IocCommonTest.MultipleConfig
{
	public class FakeAppConfigReader : IAppConfigReader
	{
		private readonly IDictionary<string, string> _data;

		public FakeAppConfigReader(IDictionary<string, string> data)
		{
			_data = data;
		}

		public FakeAppConfigReader()
		{
			_data=new Dictionary<string, string>();
		}

		public string AppConfig(string key)
		{
			string ret;
			return _data.TryGetValue(key, out ret) ? ret : null;
		}
	}
}