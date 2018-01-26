using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	public class MyTimeWebTestAttribute : IoCTestAttribute
	{
		public static string DefaultTenantName = "default";
		public static Guid DefaultBusinessUnitId = Guid.NewGuid();

		public IAuthorizationScope AuthorizationScope;
		public IAuthorization Authorization;
		public IThreadPrincipalContext PrincipalContext;
		public FakeDataSourceForTenant DataSourceForTenant;
		public IDataSourceScope DataSourceScope;
		public IPersonRepository Persons;
		public Lazy<FakeDatabase> Database;
		
		private IDisposable _tenantScope;
		private Person _loggedOnPerson;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var principalAuthorization = new FullPermission();

			CurrentAuthorization.DefaultTo(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));

			system.AddService<FakeStorage>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble(new FakePermissionProvider(false)).For<IPermissionProvider>();
			system.UseTestDouble(principalAuthorization).For<IAuthorization>();
			system.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			system.UseTestDouble<FakeSeatBookingRepository>().For<ISeatBookingRepository>();
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			system.UseTestDouble(scenarioRepository).For<IScenarioRepository>();
			system.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			system.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			system.UseTestDouble<FakePersonForScheduleFinder>().For<IPersonForScheduleFinder>();
			system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			system.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			system.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			system.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			system.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
			system.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
			system.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
			system.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			system.UseTestDouble<FakeShiftTradeLightValidator>().For<IShiftTradeLightValidator>();
			system.UseTestDouble<FakePersonContractProvider>().For<FakePersonContractProvider>();
			system.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			system.UseTestDouble<CurrentTenantFake>().For<ICurrentTenant>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateEnglishCulture())).For<IUserCulture>();
			system.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>(); 
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<ScheduleMinMaxTimeCalculator>().For<IScheduleMinMaxTimeCalculator>();
			system.UseTestDouble<SiteOpenHourProvider>().For<ISiteOpenHourProvider>();
			system.UseTestDouble<ScheduledSkillOpenHourProvider>().For<IScheduledSkillOpenHourProvider>();
			system.UseTestDouble<StaffingDataAvailablePeriodProvider>().For<IStaffingDataAvailablePeriodProvider>();

			// Tenant (and datasource) stuff
			var tenants = new FakeTenants();
			tenants.Has(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			system.UseTestDouble(tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			system.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			system.AddService<FakeDatabase>();

			//license
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			fakeSignin();
		}

		private void fakeSignin()
		{
			var signedIn = QueryAllAttributes<LoggedOffAttribute>().IsEmpty();
			if (signedIn)
			{
				_loggedOnPerson = new Person()
					.WithName(new Name("Fake", "Login"))
					.WithId();
				_loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				_loggedOnPerson.PermissionInformation.SetCulture(CultureInfoFactory.CreateEnglishCulture());
				_loggedOnPerson.PermissionInformation.SetUICulture(CultureInfoFactory.CreateEnglishCulture());

				var principal = new TeleoptiPrincipal(
					new TeleoptiIdentity(
						"Fake Login",
						DataSourceForTenant.Tenant(DefaultTenantName),
						new BusinessUnit("loggedOnBu").WithId(),
						null,
						null
					),
					_loggedOnPerson);

				PrincipalContext.SetCurrentPrincipal(principal);
			}
			else
			{
				// assuming that if you specify default data you are using FakeDatabase...
				// ... and thereby you want a current tenant for it to work...
				if (QueryAllAttributes<DefaultDataAttribute>().Any())
					_tenantScope = DataSourceScope.OnThisThreadUse(DefaultTenantName);
			}
		}
	}
}