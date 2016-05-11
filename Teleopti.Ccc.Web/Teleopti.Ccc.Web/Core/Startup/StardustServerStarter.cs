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
			var managerConfiguration = new ManagerConfiguration
			(
				connectionString : ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				route : ConfigurationManager.AppSettings["RouteName"],
				allowedNodeDownTimeSeconds : int.Parse(ConfigurationManager.AppSettings["AllowedNodeDownTimeSeconds"]),
				checkNewJobIntervalSeconds : int.Parse(ConfigurationManager.AppSettings["CheckNewJobIntervalSeconds"])
			);
			app.UseStardustManager(managerConfiguration, scope);
			new ManagerStarter().Start(managerConfiguration, scope);
		}
	}
}