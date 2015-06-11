using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new MultiTenancyModule(configuration));

			builder.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			builder.UseTestDouble<CurrentTenantCredentialsFake>().For<ICurrentTenantCredentials>();
		}
	}
}