using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	//To be moved out to seperate application
	public class TennantModule : Module
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
				var allDataSources = c.Resolve<IAvailableDataSourcesProvider>().AvailableDataSources();
				return TennantSessionManager.CreateInstanceForWeb(allDataSources.First().Application.ConnectionString);
			})
				.AsImplementedInterfaces()
				.SingleInstance();
			builder.RegisterType<TennantUnitOfWorkAspect>().SingleInstance();
		}
	}
}