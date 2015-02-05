using Autofac;
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

			if (isRunFromTest(tenantServer) || pathIsAnUrl(tenantServer))
			{
				builder.Register(c => new AuthenticationQuerier(tenantServer))
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
			return tenantServer==null;
		}

		private static bool pathIsAnUrl(string tenantServer)
		{
			return tenantServer.StartsWith("http://") || tenantServer.StartsWith("https://");
		}
	}
}