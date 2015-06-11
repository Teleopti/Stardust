using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));
			builder.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			builder.UseTestDouble<CheckPasswordStrengthFake>().For<ICheckPasswordStrength>();
			builder.UseTestDouble<DeletePersonInfoFake>().For<IDeletePersonInfo>();
			builder.UseTestDouble<ApplicationUserQueryFake>().For<IApplicationUserQuery>();
			builder.UseTestDouble<IdentityUserQueryFake>().For<IIdentityUserQuery>();
			builder.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			builder.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			builder.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			builder.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			builder.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			builder.UseTestDouble<LogLogonAttemptFake>().For<ILogLogonAttempt>();
			builder.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			builder.UseTestDouble<DataSourceConfigurationProviderFake>().For<IDataSourceConfigurationProvider>();
		}
	}
}