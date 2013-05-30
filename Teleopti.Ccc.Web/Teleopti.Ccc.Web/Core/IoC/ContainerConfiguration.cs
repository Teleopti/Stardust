using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;
using MbCache.Configuration;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{

		public IContainer Configure()
		{
			var builder = new ContainerBuilder();

			builder.RegisterControllers(Assembly.GetExecutingAssembly());

			builder.RegisterModule(new AutofacWebTypesModuleFromRepository20111123());
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();

			builder.RegisterFilterProvider();

			builder.RegisterModule<BootstrapperModule>();

			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule<MobileReportsAreaModule>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule(new InitializeModule(DataSourceConfigurationSetter.ForWeb()));
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<RuleSetModule>();

			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterType<WebRequestPrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

			registerAopComponents(builder);

			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), new FixedNumberOfLockObjects(100));
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RuleSetCacheModule(mbCacheModule, false));
			builder.RegisterModule(new AuthenticationCachedModule(mbCacheModule));

			return builder.Build();
		}

		private static void registerAopComponents(ContainerBuilder builder)
		{
			builder.RegisterModule<AspectsModule>();
			builder.RegisterType<UnitOfWorkAspect>();
		}

	}
}
