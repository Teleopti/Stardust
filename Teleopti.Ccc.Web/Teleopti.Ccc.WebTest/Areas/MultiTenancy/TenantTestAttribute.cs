using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
		}

		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			isolate.UseTestDouble<CheckPasswordStrengthFake>().For<ICheckPasswordStrength>();
			isolate.UseTestDouble<DeletePersonInfoFake>().For<IDeletePersonInfo>();
			isolate.UseTestDouble<ApplicationUserQueryFake>().For<IApplicationUserQuery>();
			isolate.UseTestDouble<IdentityUserQueryFake>().For<IIdentityUserQuery>();
			isolate.UseTestDouble<IdUserQueryFake>().For<IIdUserQuery>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			isolate.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			isolate.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<LogLogonAttemptFake>().For<ILogLogonAttempt>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble<FakePersistExternalApplicationAccess>().For<IPersistExternalApplicationAccess>();
			isolate.UseTestDouble<FakeFindExternalApplicationAccess>().For<IFindExternalApplicationAccess>();
			isolate.UseTestDouble<DataTokenManager>().For<IDataTokenManager>();
			isolate.UseTestDouble<ApplicationConfigurationDbProviderFake>().For<IApplicationConfigurationDbProvider>();
		}
	}
}