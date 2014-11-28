using System;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public interface IGlobalConfiguration
	{
		void Configure(Action<HttpConfiguration> configurationAction);
	}
}