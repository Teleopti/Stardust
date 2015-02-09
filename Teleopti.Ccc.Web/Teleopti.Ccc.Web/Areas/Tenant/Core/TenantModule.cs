using System.Linq;
using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	//To be moved out to seperate application
	public class TenantModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
			builder.RegisterType<PasswordPolicyCheck>().As<IPasswordPolicyCheck>().SingleInstance();
			builder.RegisterType<ConvertDataToOldUserDetailDomain>().As<IConvertDataToOldUserDetailDomain>().SingleInstance();
			builder.RegisterType<PasswordVerifier>().As<IPasswordVerifier>().SingleInstance();
			//ta första appdb för nu!
			builder.Register(c =>
			{
				var allDataSources = c.Resolve<IApplicationData>().RegisteredDataSourceCollection;
				return TenantUnitOfWorkManager.CreateInstanceForWeb(allDataSources.First().Application.ConnectionString);
			})
				.AsImplementedInterfaces()
				.SingleInstance();
			builder.RegisterType<TenantUnitOfWorkAspect>().SingleInstance();
			builder.RegisterType<PersistLogonAttempt>().As<IPersistLogonAttempt>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationProviderUsingNhibFiles>().As<IDataSourceConfigurationProvider>().SingleInstance();
			builder.RegisterType<ReadNHibFiles>().As<IReadNHibFiles>().SingleInstance();
			builder.RegisterType<ParseNhibFile>().As<IParseNhibFile>().SingleInstance();
		}
	}
}