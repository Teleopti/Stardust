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
			builder.Register(c => new AuthenticationQuerier(_configuration.Args().TennantServer))
				.As<IAuthenticationQuerier>()
				.SingleInstance();
		}
	}
}