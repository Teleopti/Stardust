using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class AuthenticationCachedModule : Module
	{
		private readonly MbCacheModule _mbCacheModule;

		public AuthenticationCachedModule(MbCacheModule mbCacheModule) {
			_mbCacheModule = mbCacheModule;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiPrincipalCacheableFactory>().As<IPrincipalFactory>().SingleInstance();
			builder.RegisterType<TeleoptiPrincipalInternalsFactory>()
				.IntegrateWithMbCache()
				.As<IMakeRegionalFromPerson>()
				.As<IMakeOrganisationMembershipFromPerson>()
				.As<IRetrievePersonNameForPerson>()
				.SingleInstance();

			_mbCacheModule.Builder
				.For<TeleoptiPrincipalInternalsFactory>()
				.CacheMethod(m => m.MakeOrganisationMembership(null))
				.CacheMethod(m => m.MakeRegionalFromPerson(null))
				.CacheMethod(m => m.NameForPerson(null))
				.AsImplemented()
				;

		}
	}
}