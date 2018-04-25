using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ChangePassword>().For<IChangePassword>();
			isolate.UseTestDouble<TenantPeopleSaver>().For<ITenantPeopleSaver>();
			isolate.UseTestDouble<TenantDataManager>().For<ITenantDataManager>();
			isolate.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();

			isolate.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			isolate.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			isolate.UseTestDouble<FakeCurrentTenantCredentials>().For<ICurrentTenantCredentials>();
		}
	}
}