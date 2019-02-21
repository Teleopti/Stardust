using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core
{
	[MyTimeWebTest]
	[TestFixture]
	public class MonthScheduleDomainDataProviderTest : IIsolateSystem
	{
		public FakeLoggedOnUser LoggedOnUser;
		public IMonthScheduleDomainDataProvider Target;
		public ICurrentDataSource CurrentDataSource;
		public ICurrentScenario CurrentScenario;
		public IScheduleStorage ScheduleStorage;
		public FullPermission Authorization;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeBankHolidayCalendarSiteRepository BankHolidayCalendarSiteRepository;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;
		public FakeBankHolidayDateRepository BankHolidayDateRepository;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MonthScheduleDomainDataProvider>().For<IMonthScheduleDomainDataProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<SeatOccupancyProvider>().For<ISeatOccupancyProvider>();
			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
			isolate.UseTestDouble<FakeBankHolidayCalendarSiteRepository>().For<IBankHolidayCalendarSiteRepository>();
			isolate.UseTestDouble<FakeBankHolidayDateRepository>().For<IBankHolidayDateRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldMapDate()
		{
			setupLoggedOnUser();
			var today = DateOnly.Today;

			var result = Target.GetMonthData(today);

			result.CurrentDate.Should().Be.EqualTo(today);
		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldMapDays()
		{
			setupLoggedOnUser();
			var date = new DateOnly(2014, 1, 11);
			addPersonSchedule(new DateOnly(2013, 12, 30), TimeSpan.FromHours(1), TimeSpan.FromHours(2));

			var result = Target.GetMonthData(date);

			result.Days.Count().Should().Be(35);
			result.Days.ElementAt(0).ScheduleDay.DateOnlyAsPeriod.DateOnly.Should().Be(new DateOnly(2013, 12, 30));
		}

		[Test]
		public void ShouldMapAsmEnabledToTrueWhenHavingBothLicenseAndPermission()
		{
			setupLoggedOnUser();
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.GetMonthData(today);

			result.AsmEnabled.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmLicense()
		{
			setupLoggedOnUser();
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.GetMonthData(today);

			result.AsmEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmPermission()
		{
			setupLoggedOnUser();
			PermissionProvider.Enable();
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.GetMonthData(today);

			result.AsmEnabled.Should().Be.False();
		}

		private void addPersonSchedule(DateOnly date, TimeSpan startTime, TimeSpan endTime, IActivity activity = null)
		{
			var start = DateTime.SpecifyKind(date.Date.Add(startTime), DateTimeKind.Utc);
			var end = DateTime.SpecifyKind(date.Date.Add(endTime), DateTimeKind.Utc);
			addPersonSchedule(date, start, end, activity);
		}

		private void addPersonSchedule(DateOnly date, DateTime startTime, DateTime endTime, IActivity activity = null)
		{
			activity = activity ?? new Activity().WithId();
			var assignmentPeriod = new DateTimePeriod(startTime, endTime);
			var personAssignment = new PersonAssignment(LoggedOnUser.CurrentUser(), CurrentScenario.Current(), date).WithId();
			personAssignment.AddActivity(activity, assignmentPeriod);
			ScheduleStorage.Add(personAssignment);
		}

		private void setupLoggedOnUser()
		{
			var logonUser = PersonFactory.CreatePersonWithGuid("logon", "user");
			LoggedOnUser.SetFakeLoggedOnUser(logonUser);
			PersonRepository.Add(logonUser);
			var site = new Site("site").WithId();
			SiteRepository.Add(site);
			var team = new Team { Site = site }.WithDescription(new Description("team")).WithId();
			TeamRepository.Add(team);
			logonUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));
		}
	}
}