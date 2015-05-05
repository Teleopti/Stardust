using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.LogicTest.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<PostHttpRequestFake>().As<IPostHttpRequest>().AsSelf().SingleInstance();
			builder.RegisterModule(new MultiTenancyModule(configuration));
		}
	}
}