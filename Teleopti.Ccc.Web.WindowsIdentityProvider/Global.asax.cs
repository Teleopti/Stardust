using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Auth.OpenIdApplicationStore;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new Log4NetMvCLogger());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "OpenId", action = "Identifier", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			log4net.Config.XmlConfigurator.Configure();
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);

			var builder = new ContainerBuilder();
			registerIoc(builder);

			var container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}

		private static void registerIoc(ContainerBuilder builder)
		{
			var configReader = new ConfigReader();
			builder.RegisterControllers(typeof (MvcApplication).Assembly).ApplyAspects();
			builder.RegisterType<OpenIdProviderWrapper>().As<IOpenIdProviderWrapper>().SingleInstance();
			builder.RegisterType<WindowsAccountProvider>().As<IWindowsAccountProvider>().SingleInstance();
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<Now>().As<INow>().SingleInstance();
			builder.Register(context => configReader).As<IConfigReader>().SingleInstance();
			var usePersistentCryptoKeys = ConfigurationManager.AppSettings["UsePersistentCryptoKeys"];
			if (string.IsNullOrEmpty(usePersistentCryptoKeys) || usePersistentCryptoKeys.ToLowerInvariant() != "false")
			{
				builder.RegisterType<SqlProviderApplicationStore>().As<IOpenIdApplicationStore>().SingleInstance();
				builder.RegisterType<OpenIdProvider>().SingleInstance();
			}
			else
			{
				builder.Register(context => new OpenIdProvider(OpenIdProvider.HttpApplicationStore))
					.As<OpenIdProvider>()
					.SingleInstance();
			}

			var args = new IocArgs(configReader)
			{
				DataSourceApplicationName = DataSourceApplicationName.ForWeb(),
				TeleoptiPrincipalForLegacy = true
			};
			var configuration = new IocConfiguration(args, new FalseToggleManager());
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<WindowsIpTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CryptoKeyInfoRepository>().As<ICryptoKeyInfoRepository>().SingleInstance();
			builder.RegisterType<NonceInfoRepository>().As<INonceInfoRepository>().SingleInstance();
		}
	}
}