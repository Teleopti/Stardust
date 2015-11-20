using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new MultiTenancyModule(configuration));

			system.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			system.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			system.UseTestDouble<CurrentTenantCredentialsFake>().For<ICurrentTenantCredentials>();
		}
	}
}