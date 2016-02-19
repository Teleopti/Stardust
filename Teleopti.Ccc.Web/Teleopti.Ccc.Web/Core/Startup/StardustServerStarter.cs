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

		public StardustServerStarter(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Start(IAppBuilder app)
		{
			var managerConfiguration = new ManagerConfiguration
			{
				ConnectionString =
						 ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				Route = ConfigurationManager.AppSettings["RouteName"]
			};
			
			app.UseStardustManager(managerConfiguration);

			new ManagerStarter().Start(managerConfiguration, _componentContext);
		}
	}
}