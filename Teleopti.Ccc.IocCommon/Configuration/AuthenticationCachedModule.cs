using Autofac;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class AuthenticationCachedModule : Module
	{
		private readonly IocConfiguration _configuration;

		public AuthenticationCachedModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiPrincipalCacheableFactory>().As<IPrincipalFactory>().SingleInstance();

			builder.CacheByClassProxy<TeleoptiPrincipalInternalsFactory>()
				.As<IMakeRegionalFromPerson>()
				.As<IMakeOrganisationMembershipFromPerson>()
				.As<IRetrievePersonNameForPerson>()
				.SingleInstance();
			_configuration.Args().Cache.This<TeleoptiPrincipalInternalsFactory>(b => b
				.CacheMethod(m => m.MakeOrganisationMembership(null))
				.CacheMethod(m => m.MakeRegionalFromPerson(null))
				.CacheMethod(m => m.NameForPerson(null))
				);
		}
	}
}