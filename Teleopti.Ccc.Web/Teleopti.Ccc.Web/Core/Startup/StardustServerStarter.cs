using System;
using System.Configuration;
using Autofac;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[CLSCompliant(false)]
	public class StardustServerStarter
	{
		private readonly IComponentContext  _componentContext;
		private readonly ILifetimeScope _scope;

		public StardustServerStarter(IComponentContext componentContext, ILifetimeScope scope)
		{
			_componentContext = componentContext;
			_scope = scope;
		}

		public void Start(IAppBuilder app)
		{
			var managerConfiguration = new ManagerConfiguration
			{
				ConnectionString =
						 ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				Route = ConfigurationManager.AppSettings["RouteName"]
			};
			;
			app.UseStardustManager(managerConfiguration, _scope);

			new ManagerStarter().Start(managerConfiguration, _componentContext);
		}
	}
}