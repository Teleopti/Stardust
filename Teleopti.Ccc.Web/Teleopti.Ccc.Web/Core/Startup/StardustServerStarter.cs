using System.Configuration;
using Autofac;
using Owin;
using Stardust.Manager;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class StardustServerStarter
	{
		private readonly ILifetimeScope _scope;

		public StardustServerStarter(ILifetimeScope scope)
		{
			_scope = scope;
		}

		public void Start(IAppBuilder app)
		{
			var scope = _scope.BeginLifetimeScope();
			var managerConfiguration = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				ConfigurationManager.AppSettings["RouteName"],
				int.Parse(ConfigurationManager.AppSettings["AllowedNodeDownTimeSeconds"]),
				int.Parse(ConfigurationManager.AppSettings["CheckNewJobIntervalSeconds"]),
				int.Parse(ConfigurationManager.AppSettings["PurgeBatchSize"]),
				int.Parse(ConfigurationManager.AppSettings["PurgeIntervalHours"]),
				int.Parse(ConfigurationManager.AppSettings["PurgeJobsOlderThanHours"])
				);
			app.UseStardustManager(managerConfiguration, scope);
			new ManagerStarter().Start(managerConfiguration, scope);
		}
	}
}