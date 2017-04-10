using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ChangePassword>().For<IChangePassword>();
			system.UseTestDouble<TenantPeopleSaver>().For<ITenantPeopleSaver>();
			system.UseTestDouble<TenantDataManager>().For<ITenantDataManager>();
			system.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();

			system.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			system.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			system.UseTestDouble<CurrentTenantCredentialsFake>().For<ICurrentTenantCredentials>();
		}
	}
}