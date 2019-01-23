using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	public class FakeServerConfigurationRepository : IServerConfigurationRepository
	{
		public Dictionary<string,string> Configuration = new Dictionary<string, string>();

		public IEnumerable<ServerConfiguration> AllConfigurations()
		{
			throw new NotImplementedException();
		}

		public void Update(string key, string value)
		{
			throw new NotImplementedException();
		}

		public string Get(ServerConfigurationKey key)
		{
			if (!Configuration.ContainsKey(key.ToString()))
				return "";
			return Configuration[key.ToString()];
		}
	}
}
