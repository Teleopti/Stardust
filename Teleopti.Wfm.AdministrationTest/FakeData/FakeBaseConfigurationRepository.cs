using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeBaseConfigurationRepository : IBaseConfigurationRepository
	{
		readonly IDictionary<string, IBaseConfiguration> _baseConfigurationDic = new ConcurrentDictionary<string, IBaseConfiguration>();
		public IBaseConfiguration LoadBaseConfiguration(string connectionString)
		{
			if(_baseConfigurationDic.ContainsKey(connectionString))
				return _baseConfigurationDic[connectionString];
			return new BaseConfiguration(null, null, null, false);
		}

		public void SaveBaseConfiguration(string connectionString, IBaseConfiguration configuration)
		{
			if (_baseConfigurationDic.ContainsKey(connectionString))
			{
				_baseConfigurationDic[connectionString] = configuration;
			}
			else
			{
				_baseConfigurationDic.Add(connectionString, configuration);
			}
		}
	}
}