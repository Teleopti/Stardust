using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	public class TenantSdkTestAttribute : IoCTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ChangePasswordTenantClient>().For<IChangePasswordTenantClient>();
			isolate.UseTestDouble<TenantPeopleSaver>().For<ITenantPeopleSaver>();
			isolate.UseTestDouble<TenantDataManagerClient>().For<ITenantDataManagerClient>();
			isolate.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();
			isolate.UseTestDouble<PersonDtoFactory>().For<PersonDtoFactory>();
			isolate.UseTestDouble<PersonCredentialsAppender>().For<PersonCredentialsAppender>();
			isolate.UseTestDouble<PersonAssembler>().For<IAssembler<IPerson, PersonDto>>();
			isolate.UseTestDouble<FakeWorkflowControlsetAssembler>().For<IAssembler<IWorkflowControlSet, WorkflowControlSetDto>>();
			
			isolate.UseTestDouble<FakeStorage>().For<IFakeStorage>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<PersonAccountUpdater>().For<IPersonAccountUpdater>();
			
			isolate.UseTestDouble<PostHttpRequestFake>().For<IPostHttpRequest>();
			isolate.UseTestDouble<GetHttpRequestFake>().For<IGetHttpRequest>();
			isolate.UseTestDouble<FakeCurrentTenantCredentials>().For<ICurrentTenantCredentials>();
		}

	}
}