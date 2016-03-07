using System.Configuration;
using Autofac;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Models;

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
			{
				ConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				Route = ConfigurationManager.AppSettings["RouteName"],
				AllowedNodeDownTimeSeconds = int.Parse(ConfigurationManager.AppSettings["AllowedNodeDownTimeSeconds"]),
				CheckNewJobIntervalSeconds = int.Parse(ConfigurationManager.AppSettings["CheckNewJobIntervalSeconds"])
			};
			app.UseStardustManager(managerConfiguration, scope);
			new ManagerStarter().Start(managerConfiguration, scope);
		}
	}
}