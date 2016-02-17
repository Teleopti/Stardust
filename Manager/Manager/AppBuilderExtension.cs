using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public static class AppBuilderExtension
	{

		public static void UseStardustManager(this IAppBuilder appBuilder, ManagerConfiguration managerConfiguration, ILifetimeScope lifetimeScope)
		{
			string routeName = managerConfiguration.routeName;

			appBuilder.UseDefaultFiles(new DefaultFilesOptions
			{
				FileSystem = new PhysicalFileSystem(@".\StardustDashboard"),
				RequestPath = new PathString("/StardustDashboard")
			});

			appBuilder.UseStaticFiles();

		}
	}
}
