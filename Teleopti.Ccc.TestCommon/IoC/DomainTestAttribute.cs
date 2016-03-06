using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
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

			// Tenant (and datasource) stuff
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<FakeTenants>().For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName>();
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			system.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			//

			// Event stuff
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			QueryAllAttributes<UseEventPublisherAttribute>()
				.ForEach(a => system.UseTestDoubleForType(a.EventPublisher).For<IEventPublisher>());
			system.UseTestDouble<FakeRecurringEventPublisher>().For<IRecurringEventPublisher>();
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
			system.UseTestDouble<FakeRtaDatabase>().For<IDatabaseReader>();
			system.UseTestDouble<FakeAgentStateReadModelStorage>().For<IAgentStateReadModelReader, IAgentStateReadModelPersister>();
			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister, IAdherenceDetailsReadModelReader>();
			system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister, IAdherencePercentageReadModelReader>();
			//

			system.UseTestDouble<FakeScheduleDictionaryPersister>().For<IScheduleDictionaryPersister>();
			system.UseTestDouble<FakeGroupScheduleGroupPageDataProvider>().For<IGroupScheduleGroupPageDataProvider>();

			// Repositories
			system.AddService<FakeDatabase>();
			system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			system.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
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
			system.UseTestDouble<FakeRtaMapRepository>().For<IRtaMapRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble<FakeLoadAllSkillInIntradays>().For<ILoadAllSkillInIntradays>();
			system.UseTestDouble<FakeSkillAreaRepository>().For<ISkillAreaRepository>();
			system.UseTestDouble<FakeIntradayMonitorDataLoader>().For<IIntradayMonitorDataLoader>();

			// license stuff, have to fake the ILicenseVerifierFactory too!
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository>();
			system.UseTestDouble<FakeLicenseVerifierFactory>().For<ILicenseVerifierFactory>();

			fakePrincipal(system);
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
				{
					var person = new Person { Name = new Name("Fake", "Login") };
					person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
					person.PermissionInformation.SetCulture(CultureInfoFactory.CreateEnglishCulture());
					person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateEnglishCulture());
					thePrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("Fake Login", null, null, null, null), person);
				}
				principal.Fake(thePrincipal);
			}
			system.UseTestDouble(principal).For<ICurrentTeleoptiPrincipal, ICurrentPrincipalContext>();
		}

	}

	public class FakeLicenseVerifierFactory : ILicenseVerifierFactory
	{
		private readonly ILicenseRepository _licenseRepository;

		public FakeLicenseVerifierFactory(ILicenseRepository licenseRepository)
		{
			_licenseRepository = licenseRepository;
		}

		public ILicenseVerifier Create(
			ILicenseFeedback licenseFeedback,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new LicenseVerifier(
				licenseFeedback,
				unitOfWorkFactory,
				_licenseRepository
				);
		}
	}

	public class FakeLicenseRepository : ILicenseRepository
	{
		public void Add(ILicense root)
		{
			throw new NotImplementedException();
		}

		public void Remove(ILicense root)
		{
			throw new NotImplementedException();
		}

		public ILicense Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ILicense> LoadAll()
		{
			var license = new License { XmlString = System.IO.File.ReadAllText("license.xml") };
			return new ILicense[] {license}.ToList();
		}

		public ILicense Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ILicense> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; set; }
		public IList<ActiveAgent> GetActiveAgents()
		{
			throw new NotImplementedException();
		}
	}
}