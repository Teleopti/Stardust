using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			builder.RegisterType<NhibConfigDecryption>().As<INhibConfigDecryption>().SingleInstance();

			var configServer = _configuration.Args().ConfigServer;
			if(isRunFromTest(configServer) || configServer.IsAnUrl())
			{
				builder.Register(c => new SharedSettingsQuerier(configServer))
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
			else
			{
				builder.Register(c => new SharedSettingsQuerierForNoWeb(configServer))
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
			if (_configuration.Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049))
			{
				builder.RegisterType<ChangeUserPassword>().As<IChangeUserPassword>().SingleInstance();
			}
			else
			{
				builder.RegisterType<EmptyChangeUserPassword>().As<IChangeUserPassword>().SingleInstance();
			}
		}

		private static bool isRunFromTest(string server)
		{
			return server == null;
		}
	}
}