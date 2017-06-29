using Autofac;
using log4net;
using Owin;
using Stardust.Manager;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class StardustServerStarter
	{
		private readonly ILifetimeScope _scope;
		private readonly IConfigReader _configReader;
		private static readonly ILog logger = LogManager.GetLogger(typeof(StardustServerStarter));

		public StardustServerStarter(ILifetimeScope scope, IConfigReader configReader)
		{
			_scope = scope;
			_configReader = configReader;
		}

		public void Start(IAppBuilder app)
		{
			logger.Info("StardustServerStarter.start()");
			var managerConfiguration = new ManagerConfiguration(
				_configReader.ConnectionString("ManagerConnectionString"),
				_configReader.ReadValue("RouteName", "/StardustDashboard"),
				_configReader.ReadValue("AllowedNodeDownTimeSeconds", 360), //heartbeat every 120 s
				_configReader.ReadValue("CheckNewJobIntervalSeconds", 180),
				_configReader.ReadValue("PurgeJobsBatchSize", 1000),
				_configReader.ReadValue("PurgeJobsIntervalHours", 1),
				_configReader.ReadValue("PurgeJobsOlderThanHours", 168),
				_configReader.ReadValue("PurgeNodesIntervalHours", 1));

			app.UseStardustManager(managerConfiguration, _scope);
		}
	}
}