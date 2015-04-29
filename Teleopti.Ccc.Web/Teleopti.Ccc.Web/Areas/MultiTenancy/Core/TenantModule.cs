using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public TenantModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		private const string tenancyConnectionStringKey = "Tenancy";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			if (_configuration.Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049))
			{
				builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
				builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			}
			else
			{
				builder.RegisterType<IdentityUserQuery_OldSchema>().As<IIdentityUserQuery>().SingleInstance();
				builder.RegisterType<ApplicationUserQueryOldSchema>().As<IApplicationUserQuery>().SingleInstance();
			}
			builder.RegisterType<FindTenantAndPersonIdForIdentity>().As<IFindTenantAndPersonIdForIdentity>().SingleInstance();
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
				var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
				return TenantUnitOfWork.CreateInstanceForWeb(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<TenantUnitOfWorkAspect>().As<ITenantUnitOfWorkAspect>().SingleInstance();
			builder.RegisterType<PersistLogonAttempt>().As<IPersistLogonAttempt>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationProviderUsingNhibFiles>().As<IDataSourceConfigurationProvider>().SingleInstance();
			builder.RegisterType<ReadNHibFiles>().As<IReadNHibFiles>().SingleInstance();
			builder.RegisterType<ParseNhibFile>().As<IParseNhibFile>().SingleInstance();
			builder.RegisterType<NhibConfigurationEncryption>().As<INhibConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersistPersonInfo>().As<IPersistPersonInfo>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<FindTenantByNameQuery>().As<IFindTenantByNameQuery>().SingleInstance();
			builder.RegisterType<DeletePersonInfo>().As<IDeletePersonInfo>().SingleInstance();
			builder.RegisterType<VerifyPasswordPolicy>().As<IVerifyPasswordPolicy>().SingleInstance();
			builder.RegisterType<CheckPasswordStrength>().As<ICheckPasswordStrength>().SingleInstance();
			builder.RegisterType<ChangePersonPassword>().As<IChangePersonPassword>().SingleInstance();
			builder.RegisterType<FindLogonInfo>().As<IFindLogonInfo>().SingleInstance();
		}
	}
}