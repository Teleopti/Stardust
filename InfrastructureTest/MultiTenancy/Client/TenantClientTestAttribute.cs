using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantClientTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			system.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<LoadUserUnauthorizedFake>().For<ILoadUserUnauthorized>();
			system.UseTestDouble<CurrentTenantCredentialsFake>().For<ICurrentTenantCredentials>();
		}
	}
}