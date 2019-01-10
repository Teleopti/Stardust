using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantClientTestAttribute : IoCTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			isolate.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<LoadUserUnauthorizedFake>().For<ILoadUserUnauthorized>();
			isolate.UseTestDouble<FakeCurrentTenantCredentials>().For<ICurrentTenantCredentials>();
			isolate.UseTestDouble<FakeStorage>().For<IFakeStorage>();
		}
	}
}