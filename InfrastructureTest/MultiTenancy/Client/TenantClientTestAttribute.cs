using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantClientTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<MultiTenancyApplicationLogon>().As<IMultiTenancyApplicationLogon>().SingleInstance();
			builder.RegisterType<MultiTenancyWindowsLogon>().As<IMultiTenancyWindowsLogon>().SingleInstance();
			builder.RegisterType<PostHttpRequestFake>().As<IPostHttpRequest>().AsSelf().SingleInstance();
			builder.RegisterType<VerifyTerminalDateFake>().As<IVerifyTerminalDate>().AsSelf().SingleInstance();
			builder.RegisterType<FakePersonRepository>().As<IPersonRepository>().AsSelf().SingleInstance();
			builder.RegisterType<LoadUserUnauthorizedFake>().As<ILoadUserUnauthorized>().AsSelf().SingleInstance();
			builder.RegisterType<FakeWindowUserProvider>().As<IWindowsUserProvider>().AsSelf().SingleInstance();
		}
	}
}