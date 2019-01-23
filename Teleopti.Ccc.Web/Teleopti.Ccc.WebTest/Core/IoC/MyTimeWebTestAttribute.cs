using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
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
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.IoC
{
	public class MyTimeWebTestAttribute : IoCTestAttribute
	{
		public static string DefaultTenantName = "default";
		public static Guid DefaultBusinessUnitId = Guid.NewGuid();

		public IAuthorizationScope AuthorizationScope;
		public IAuthorization Authorization;
		public IPrincipalFactory PrincipalFactory;
		public IThreadPrincipalContext PrincipalContext;
		public FakeDataSourceForTenant DataSourceForTenant;
		public IDataSourceScope DataSourceScope;
		public IPersonRepository Persons;
		public Lazy<FakeDatabase> Database;

		private IDisposable _tenantScope;
		private Person _loggedOnPerson;

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));

			extend.AddService<FakeStorage>();
			extend.AddService<FakeDatabase>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakePermissionProvider(false)).For<IPermissionProvider>();
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
			isolate.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			isolate.UseTestDouble<FakeSeatBookingRepository>().For<ISeatBookingRepository>();
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			isolate.UseTestDouble(scenarioRepository).For<IScenarioRepository>();
			isolate.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			isolate.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			isolate.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			isolate.UseTestDouble<FakePersonScheduleDayReadModelFinder>().For<IPersonScheduleDayReadModelFinder>();
			isolate.UseTestDouble<FakePersonForScheduleFinder>().For<IPersonForScheduleFinder>();
			isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			isolate.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			isolate.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			isolate.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			isolate.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
			isolate.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
			isolate.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			isolate.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
			isolate.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<FakeShiftTradeLightValidator>().For<IShiftTradeLightValidator>();
			isolate.UseTestDouble<FakePersonContractProvider>().For<FakePersonContractProvider>();
			isolate.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			isolate.UseTestDouble<CurrentTenantFake>().For<ICurrentTenant>();
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateEnglishCulture())).For<IUserCulture>();
			isolate.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>();
			isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			isolate.UseTestDouble<ScheduleWeekMinMaxTimeCalculator>().For<IScheduleWeekMinMaxTimeCalculator>();
			isolate.UseTestDouble<SiteOpenHourProvider>().For<ISiteOpenHourProvider>();
			isolate.UseTestDouble<StaffingDataAvailablePeriodProvider>().For<IStaffingDataAvailablePeriodProvider>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>(); 
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>(); 
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>(); 
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>(); 
			isolate.UseTestDouble<FakeGamificationSettingRepository>().For<IGamificationSettingRepository>(); 
			isolate.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
			isolate.UseTestDouble<FakeAgentBadgeTransactionRepository>().For<IAgentBadgeTransactionRepository>();
			isolate.UseTestDouble<FakeAgentBadgeWithRankTransactionRepository>().For<IAgentBadgeWithRankTransactionRepository>();
			isolate.UseTestDouble<SkillIntradayStaffingFactory>().For<SkillIntradayStaffingFactory>();

			// Tenant (and datasource) stuff
			var tenants = new FakeTenants();
			tenants.Has(DefaultTenantName, "a key");
			isolate.UseTestDouble(tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			isolate.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();

			//license
			isolate.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();
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

				var principal = PrincipalFactory.MakePrincipal(
					_loggedOnPerson,
					DataSourceForTenant.Tenant(DefaultTenantName),
					new BusinessUnit("loggedOnBu").WithId(),
					null
				);
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

		protected override void AfterTest()
		{
			base.AfterTest();

			_tenantScope?.Dispose();
			Authorization = null;
			AuthorizationScope = null;
			DataSourceForTenant?.Dispose();
			DataSourceForTenant = null;
			DataSourceScope = null;
			Database = null;
			Persons = null;
			Now = null;
		}
	}
}