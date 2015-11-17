using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class DomainTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: move this to common
			system.AddModule(new SchedulingCommonModule(configuration));
			system.AddModule(new RuleSetModule(configuration, true));
			system.AddModule(new OutboundScheduledResourcesProviderModule());
			//

			fakePrincipal(system);

			// Tenant (and datasource) stuff
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<FakeTenants>().For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName>();
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			system.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			//

			// Outbound stuff
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			//

			// Database aspects
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();
			system.UseTestDouble<FakeAllBusinessUnitsUnitOfWorkAspect>().For<IAllBusinessUnitsUnitOfWorkAspect>();
			system.UseTestDouble<FakeDistributedLockAcquirer>().For<IDistributedLockAcquirer>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeMessageBrokerUnitOfWorkAspect>().For<IMessageBrokerUnitOfWorkAspect>();
			//

			// Messaging ztuff 
			system.UseTestDouble<FakeSignalR>().For<ISignalR>();
			//system.UseTestDouble<ActionImmediate>().For<IActionScheduler>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
			//

			// Permission stuff
			system.UseTestDouble<PrincipalAuthorizationWithFullPermission>().For<IPrincipalAuthorization>();
			//

			// Rta
			system.UseTestDouble<FakeRtaDatabase>().For<IDatabaseReader, IDatabaseWriter>();
			system.UseTestDouble<FakeAgentStateReadModelReader>().For<IAgentStateReadModelReader>();
			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister>();
			system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister>();
			//

			system.UseTestDouble<FakeScheduleDictionaryPersister>().For<IScheduleDictionaryPersister>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
			system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			system.UseTestDouble<FakeScheduleTagRepository>().For<IScheduleTagRepository>();
			system.UseTestDouble<FakeWorkflowControlSetRepository>().For<IWorkflowControlSetRepository>();
			system.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			system.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			system.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
			system.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
			system.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
			system.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
			system.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakeDayOffRulesRepository>().For<IDayOffRulesRepository>();
			system.UseTestDouble<FakeStatisticRepository>().For<IStatisticRepository>();
			system.UseTestDouble<FakeRtaStateGroupRepository>().For<IRtaStateGroupRepository>();
			system.UseTestDouble<FakeStateGroupActivityAlarmRepository>().For<IStateGroupActivityAlarmRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
		}

		private void fakePrincipal(ISystem system)
		{
			var principal = new FakeCurrentTeleoptiPrincipal();
			var signedIn = QueryAllAttributes<LoggedOffAttribute>().IsEmpty();
			if (signedIn)
			{
				// because DomainTest project has a SetupFixtureForAssembly that creates a principal and sets it to that static thing... 
				var thePrincipal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
				if (thePrincipal == null)
					// add more when required, but we have to sure its the correct "things" added
					// for example, which datasource? if the test creates one, it should probably be that one...
					// and for businessunit, it should probably not just be just "newing up" one
					// MAYBE the principal should be create on demand (Func<ITeleoptiPrincipal>), adding whatever is the ICurrent* stuff at the time
					thePrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null, null), null);
				principal.Fake(thePrincipal);
			}
			system.UseTestDouble(principal).For<ICurrentTeleoptiPrincipal>();
		}
	}
}