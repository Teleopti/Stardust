using System;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest
{
	public class FakeBaseConfigurationRepository : IBaseConfigurationRepository
	{
		public IBaseConfiguration LoadBaseConfiguration(string connectionString)
		{
			return new BaseConfiguration(null, null, null, false);
		}

		public void SaveBaseConfiguration(string connectionString, IBaseConfiguration configuration)
		{
			throw new NotImplementedException();
		}
	}
}