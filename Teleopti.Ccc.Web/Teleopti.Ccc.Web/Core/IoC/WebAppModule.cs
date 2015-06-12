
using System.Web.Http;
using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebAppModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebAppModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		public WebAppModule(IIocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterModule(new CommonModule(_configuration));
			builder.RegisterModule(new WebModule(_configuration, _httpConfiguration));

			tenantWebSpecificTypes(builder);
		}

		private static void tenantWebSpecificTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationProvider>().As<IDataSourceConfigurationProvider>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationEncryption>().As<IDataSourceConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<ChangePersonPassword>().As<IChangePersonPassword>().SingleInstance();
			builder.RegisterType<TenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CurrentTenantUser>().As<ICurrentTenantUser>().SingleInstance();
			builder.RegisterType<NHibFilePathInWeb>().As<INhibFilePath>().SingleInstance();
		}
	}
}