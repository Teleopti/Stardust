using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface IServerConfigurationRepository
	{
		IEnumerable<ServerConfiguration> AllConfigurations();
		void Update(string key, string value);
		string Get(string key);
	}
}