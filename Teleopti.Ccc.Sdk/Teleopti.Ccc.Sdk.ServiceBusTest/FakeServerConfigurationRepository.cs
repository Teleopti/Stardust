using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
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

		public string Get(string key)
		{
			if (!Configuration.ContainsKey(key))
				return "";
			return Configuration[key];
		}
	}
}
