using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class AuthenticationCachedModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public AuthenticationCachedModule(IIocConfiguration configuration)
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
			_configuration.Cache().This<TeleoptiPrincipalInternalsFactory>(b => b
				.CacheMethod(m => m.MakeOrganisationMembership(null))
				.CacheMethod(m => m.MakeRegionalFromPerson(null))
				.CacheMethod(m => m.NameForPerson(null))
				);
		}
	}
}