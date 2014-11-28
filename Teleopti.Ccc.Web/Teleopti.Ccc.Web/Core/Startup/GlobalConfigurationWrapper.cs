using System;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class GlobalConfigurationWrapper : IGlobalConfiguration
	{
		public void Configure(Action<HttpConfiguration> configurationAction)
		{
			GlobalConfiguration.Configure(configurationAction);
		}
	}
}