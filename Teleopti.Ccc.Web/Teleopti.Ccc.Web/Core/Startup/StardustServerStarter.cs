using System.Configuration;
using Autofac;
using Owin;
using Stardust.Manager;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class StardustServerStarter
	{
		private readonly IComponentContext _componentContext;
		private readonly ILifetimeScope _scope;
		private readonly IConfigReader _configReader;

		public StardustServerStarter(IComponentContext componentContext, ILifetimeScope scope, IConfigReader configReader)
		{
			_componentContext = componentContext;
			_scope = scope;
			_configReader = configReader;
		}

		public void Start(IAppBuilder app)
		{
			var managerConfiguration = new ManagerConfiguration(
				_configReader.ConnectionString("ManagerConnectionString"),
				_configReader.ReadValue("RouteName", "/StardustDashboard"),
				_configReader.ReadValue("AllowedNodeDownTimeSeconds", 60),
				_configReader.ReadValue("CheckNewJobIntervalSeconds", 180),
				_configReader.ReadValue("PurgeJobsBatchSize", 1000),
				_configReader.ReadValue("PurgeJobsIntervalHours", 1),
				_configReader.ReadValue("PurgeJobsOlderThanHours", 168),
				_configReader.ReadValue("PurgeNodesIntervalHours", 1));

			app.UseStardustManager(managerConfiguration, _scope);
			new ManagerStarter().Start(managerConfiguration, _componentContext);
		}
	}
}