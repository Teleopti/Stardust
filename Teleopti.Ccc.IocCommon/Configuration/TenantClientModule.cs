using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class TenantClientModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public TenantClientModule(IIocConfiguration configuration) 
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var tenantServer = _configuration.Args().TenantServer;
			builder.Register(c => new TenantServerConfiguration(tenantServer)).As<ITenantServerConfiguration>().SingleInstance();
			if (isRunFromTest(tenantServer) || tenantServer.IsAnUrl())
			{
				builder.RegisterType<AuthenticationQuerier>().As<IAuthenticationQuerier>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AuthenticationFromFileQuerier>().As<IAuthenticationQuerier>().SingleInstance();
			}
			builder.RegisterType<PostHttpRequest>().As<IPostHttpRequest>().SingleInstance();
			builder.RegisterType<NhibConfigEncryption>().As<INhibConfigEncryption>().SingleInstance();

			var configServer = _configuration.Args().ConfigServer;
			if(isRunFromTest(configServer) || configServer.IsAnUrl())
			{
				builder.Register(c => new SharedSettingsQuerier(configServer))
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
			else
			{
				builder.Register(c => new SharedSettingsQuerierForNoWeb())
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
		}

		private static bool isRunFromTest(string server)
		{
			return server == null;
		}
	}
}