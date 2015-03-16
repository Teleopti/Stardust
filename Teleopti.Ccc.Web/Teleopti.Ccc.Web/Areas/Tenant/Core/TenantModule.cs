using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class TenantModule : Module
	{
		private const string tenancyConnectionStringKey = "Tenancy";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			builder.RegisterType<ApplicationUserTenantQuery>().As<IApplicationUserTenantQuery>().SingleInstance();
			builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
			builder.RegisterType<FindTenantAndPersonIdForIdentity>().As<IFindTenantAndPersonIdForIdentity>().SingleInstance();
			builder.RegisterType<PasswordPolicyCheck>().As<IPasswordPolicyCheck>().SingleInstance();
			builder.RegisterType<ConvertDataToOldUserDetailDomain>().As<IConvertDataToOldUserDetailDomain>().SingleInstance();
			builder.RegisterType<PasswordVerifier>().As<IPasswordVerifier>().SingleInstance();
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
				var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
				return TenantUnitOfWorkManager.CreateInstanceForWeb(connstringAsString);
			})
				.As<ITenantUnitOfWorkManager>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<TenantUnitOfWorkAspect>().SingleInstance();
			builder.RegisterType<PersistLogonAttempt>().As<IPersistLogonAttempt>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationProviderUsingNhibFiles>().As<IDataSourceConfigurationProvider>().SingleInstance();
			builder.RegisterType<ReadNHibFiles>().As<IReadNHibFiles>().SingleInstance();
			builder.RegisterType<ParseNhibFile>().As<IParseNhibFile>().SingleInstance();
			builder.RegisterType<NhibConfigurationEncryption>().As<INhibConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersistPersonInfo>().As<IPersistPersonInfo>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<FindTenantByNameQuery>().As<IFindTenantByNameQuery>().SingleInstance();
			builder.RegisterType<DeletePersonInfo>().As<IDeletePersonInfo>().SingleInstance();
		}
	}
}