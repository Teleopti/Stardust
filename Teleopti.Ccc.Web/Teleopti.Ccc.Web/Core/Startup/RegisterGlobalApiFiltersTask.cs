using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(10)]
	public class RegisterGlobalApiFiltersTask : IBootstrapperTask
	{
		private readonly IGlobalConfiguration _globalConfiguration;
		private readonly Log4NetLogger _log4NetLogger;

		public RegisterGlobalApiFiltersTask(IGlobalConfiguration globalConfiguration, Log4NetLogger log4NetLogger)
		{
			_globalConfiguration = globalConfiguration;
			_log4NetLogger = log4NetLogger;
		}

		public Task Execute(IAppBuilder application)
		{
			_globalConfiguration.Configure(c => c.Filters.Add(new AuthorizeTeleoptiAttribute(new []
			{
				typeof (DangerousApiController),
				typeof (MessageBrokerController),
			})));
			_globalConfiguration.Configure(c => c.Filters.Add(new Log4NetWebApiLogger(_log4NetLogger)));
			return Task.FromResult(false);
		}
	}
}