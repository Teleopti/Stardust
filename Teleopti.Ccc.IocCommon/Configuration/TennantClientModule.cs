using Autofac;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class TennantClientModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public TennantClientModule(IIocConfiguration configuration) 
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var tennantServer = _configuration.Args().TennantServer;

			if (isRunFromTest(tennantServer) || pathIsAnUrl(tennantServer))
			{
				builder.Register(c => new AuthenticationQuerier(tennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
			else
			{
				builder.Register(c => new AuthenticationFromFileQuerier(tennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
		}

		private static bool isRunFromTest(string tennantServer)
		{
			return tennantServer==null;
		}

		private static bool pathIsAnUrl(string tennantServer)
		{
			return tennantServer.StartsWith("http://") || tennantServer.StartsWith("https://");
		}
	}
}