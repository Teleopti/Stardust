using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class DomainTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			// Tenant stuff
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
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
			//

			//Permission stuff
			system.UseTestDouble<PrincipalAuthorizationWithFullPermission>().For<IPrincipalAuthorization>();
			//

			//TODO: move this to common
			system.AddModule(new SchedulingCommonModule(configuration));
			system.AddModule(new RuleSetModule(configuration, true));
			system.AddModule(new OutboundScheduledResourcesProviderModule());
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
			system.UseTestDouble<FakeWorkRuleSettingsRepository>().For<IWorkRuleSettingsRepository>();
			system.UseTestDouble<FakeStatisticRepository>().For<IStatisticRepository>();
		}
	}
}