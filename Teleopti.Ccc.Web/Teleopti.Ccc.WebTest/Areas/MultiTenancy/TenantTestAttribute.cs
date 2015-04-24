using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));
			builder.RegisterType<PersistPersonInfoFake>().As<IPersistPersonInfo>().AsSelf().SingleInstance();
			builder.RegisterType<FindTenantByNameQueryFake>().As<IFindTenantByNameQuery>().AsSelf().SingleInstance();
			builder.RegisterType<CheckPasswordStrengthFake>().As<ICheckPasswordStrength>().AsSelf().SingleInstance();
			builder.RegisterType<DeletePersonInfoFake>().As<IDeletePersonInfo>().AsSelf().SingleInstance();
			builder.RegisterType<ApplicationUserTenantQueryFake>().As<IApplicationUserTenantQuery>().AsSelf().SingleInstance();
			builder.RegisterType<TenantUnitOfWorkFake>().As<ITenantUnitOfWork>().AsSelf().SingleInstance();
		}
	}
}