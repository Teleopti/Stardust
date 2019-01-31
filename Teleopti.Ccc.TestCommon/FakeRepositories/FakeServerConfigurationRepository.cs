using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeServerConfigurationRepository : IServerConfigurationRepository
	{
		private readonly Dictionary<ServerConfigurationKey, string> _configuration = new Dictionary<ServerConfigurationKey, string>();

		public IEnumerable<ServerConfiguration> AllConfigurations()
		{
			throw new NotImplementedException();
		}

		public void Update(string key, string value)
		{
			_configuration[(ServerConfigurationKey) Enum.Parse(typeof(ServerConfigurationKey), key)] = value;
		}

		public string Get(ServerConfigurationKey key)
		{
			return !_configuration.TryGetValue(key, out var val) ? "" : val;
		}
	}
}