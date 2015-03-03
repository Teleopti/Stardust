using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

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

			if (isRunFromTest(tenantServer) || tenantServer.IsAnUrl())
			{
				builder.Register(c => new AuthenticationQuerier(tenantServer, new NhibConfigEncryption()))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
			else
			{
				builder.Register(c => new AuthenticationFromFileQuerier(tenantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
		}

		private static bool isRunFromTest(string tenantServer)
		{
			return tenantServer == null;
		}
	}
}