using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	//To be moved out to seperate application
	public class TenantModule : Module
	{
		private const string tenancyConnectionStringKey = "Tenancy";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
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
		}
	}
}