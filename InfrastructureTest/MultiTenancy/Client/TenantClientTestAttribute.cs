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
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			builder.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			builder.UseTestDouble<LoadUserUnauthorizedFake>().For<ILoadUserUnauthorized>();
			builder.UseTestDouble<CurrentTenantCredentialsFake>().For<ICurrentTenantCredentials>();
		}
	}
}