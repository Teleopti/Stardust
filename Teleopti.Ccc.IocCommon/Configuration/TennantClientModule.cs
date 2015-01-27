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

			if (string.IsNullOrEmpty(tennantServer))
			{

				builder.Register(c => new AuthenticationQuerier(_configuration.Args().TennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
			else if(tennantPathIsAnUrl())
			{
				builder.Register(c => new AuthenticationQuerier(_configuration.Args().TennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
			else
			{
				builder.Register(c => new AuthenticationFromFileQuerier(_configuration.Args().TennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
			}
		}

		private bool tennantPathIsAnUrl()
		{
			var tennantServer = _configuration.Args().TennantServer;
			return tennantServer.StartsWith("http://") || tennantServer.StartsWith("https://");
		}
	}
}