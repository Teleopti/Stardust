using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core
{
	[MyTimeWebTest]
	[TestFixture]
	public class DayScheduleDomainDataProviderDayScheduleTest : IIsolateSystem
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeBudgetDayRepository BudgetDayRepository;
		public IScheduleStorage ScheduleStorage;
		public IDayScheduleDomainDataProvider Target;
		public ICurrentScenario CurrentScenario;
		public FakePersonRequestRepository PersonRequestRepository;
		public ICurrentDataSource CurrentDataSource;
		public MutableNow Now;
		public Areas.Global.FakePermissionProvider PermissionProvider;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario { DefaultScenario = true })).For<IScenarioRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
		}

		[Test]
		public void ShouldMapDateOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			var result = Target.GetDaySchedule(date);
			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapAScheduleDayOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			var result = Target.GetDaySchedule(date);
			result.Should().Not.Be(null);
		}

		[Test]
		public void ShouldMapScheduleDayOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			addPersonSchedule(date.AddDays(-1), TimeSpan.Zero, TimeSpan.FromDays(1));
			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(2));

			var result = Target.GetDaySchedule(date);
			result.ScheduleDay.PersonAssignment().Period.StartDateTime.Should().Be(date.Date.AddHours(1));
			result.ScheduleDay.PersonAssignment().Period.EndDateTime.Should().Be(date.Date.AddHours(2));
		}

		[Test]
		public void ShouldMapProjectionOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(2));

			var result = Target.GetDaySchedule(date);
			result.Projection.ElementAt(0).Period.StartDateTime.Should().Be(date.Date.AddHours(1));
			result.Projection.ElementAt(0).Period.EndDateTime.Should().Be(date.Date.AddHours(2));
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			var overtimeAvailability = new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(1, 0, 0),
				new TimeSpan(2, 0, 0)).WithId();
			ScheduleStorage.Add(overtimeAvailability);

			var result = Target.GetDaySchedule(date);

			result.OvertimeAvailability.Should().Be(overtimeAvailability);
		}
		
		[Test]
		public void ShouldMapPersonRequestsOnGetDaySchedule()
		{
			var date = new DateOnly(DateTime.UtcNow.Date);

			var personRequest = new PersonRequest(new Person(),
				new TextRequest(
					new DateTimePeriod(
						DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
				));
			PersonRequestRepository.Add(personRequest);

			var result = Target.GetDaySchedule(date);

			result.PersonRequestCount.Should().Be(1);
		}

		[Test]
		public void ShouldMapClassOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			Now.Is(date.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);

			setupWorkFlowControlSet();

			var user = LoggedOnUser.CurrentUser();
			var team = TeamFactory.CreateTeam("team1", "site1");
			var budgetGroup = new BudgetGroup().WithId();
			var personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(firstDayOfWeek.AddDays(-1), PersonContractFactory.CreatePersonContract(), team);
			personPeriod.BudgetGroup = budgetGroup;
			user.AddPersonPeriod(personPeriod);

			var absenceRequestOpenDatePeriod = user.WorkflowControlSet.AbsenceRequestOpenPeriods[0] as AbsenceRequestOpenDatePeriod;
			absenceRequestOpenDatePeriod.StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
			absenceRequestOpenDatePeriod.Period = week;
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = week;

			for (var i = 0; i < 7; i++)
			{
				var budgetDay = new BudgetDay(budgetGroup, CurrentScenario.Current(), week.StartDate.AddDays(i));
				BudgetDayRepository.Add(budgetDay);
			}

			var result = Target.GetDaySchedule(date);
			result.ProbabilityClass.Should().Be.EqualTo(BudgetCssClass.Poor);
		}

		[Test]
		public void ShouldMapTextOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			Now.Is(date.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);

			setupWorkFlowControlSet();

			var user = LoggedOnUser.CurrentUser();
			var team = TeamFactory.CreateTeam("team1", "site1");
			var budgetGroup = new BudgetGroup().WithId();
			var personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(firstDayOfWeek.AddDays(-1), PersonContractFactory.CreatePersonContract(), team);
			personPeriod.BudgetGroup = budgetGroup;
			user.AddPersonPeriod(personPeriod);

			var absenceRequestOpenDatePeriod = user.WorkflowControlSet.AbsenceRequestOpenPeriods[0] as AbsenceRequestOpenDatePeriod;
			absenceRequestOpenDatePeriod.StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
			absenceRequestOpenDatePeriod.Period = week;
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = week;

			for (var i = 0; i < 7; i++)
			{
				var budgetDay = new BudgetDay(budgetGroup, CurrentScenario.Current(), week.StartDate.AddDays(i));
				BudgetDayRepository.Add(budgetDay);
			}

			var result = Target.GetDaySchedule(date);
			result.ProbabilityText.Should().Be.EqualTo("Poor");
		}

		[Test]
		public void ShouldMapAvailabilityOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			Now.Is(date.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);

			setupWorkFlowControlSet();

			var user = LoggedOnUser.CurrentUser();
			var team = TeamFactory.CreateTeam("team1", "site1");
			var budgetGroup = new BudgetGroup().WithId();
			var personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(firstDayOfWeek.AddDays(-1), PersonContractFactory.CreatePersonContract(), team);
			personPeriod.BudgetGroup = budgetGroup;
			user.AddPersonPeriod(personPeriod);

			var absenceRequestOpenDatePeriod = user.WorkflowControlSet.AbsenceRequestOpenPeriods[0] as AbsenceRequestOpenDatePeriod;
			absenceRequestOpenDatePeriod.StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
			absenceRequestOpenDatePeriod.Period = week;
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = week;

			for (var i = 0; i < 7; i++)
			{
				var budgetDay = new BudgetDay(budgetGroup, CurrentScenario.Current(), week.StartDate.AddDays(i));
				BudgetDayRepository.Add(budgetDay);
			}

			var result = Target.GetDaySchedule(date);
			result.Availability.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMapPersonRequestsStartingAtMidnightOnGetDaySchedule()
		{
			var date = DateOnly.Today;
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);

			var personRequest = new PersonRequest(new Person(),
				new TextRequest(
					new DateTimePeriod(
						localMidnightInUtc, localMidnightInUtc.AddHours(1))
				));
			PersonRequestRepository.Add(personRequest);

			var result = Target.GetDaySchedule(date);

			result.PersonRequestCount.Should().Be(1);
		}

		[Test]
		public void ShouldMapMinMaxTimeOnGetDaySchedule()
		{
			var date = DateOnly.Today;

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(8), localMidnightInUtc.AddHours(17));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(7);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(17);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShiftOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 28);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldHaveCorrectStartTimeForEndingAtMidNightEdgeCaseOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 28);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(24));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShiftStartingOnTheLastDayOfCurrentWeekOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityOnGetDaySchedule()
		{
			var date = new DateOnly(2013, 09, 11);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(2);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftOnGetDaySchedule()
		{
			var date = new DateOnly(2013, 09, 11);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvernightOvertimeAvailabilityThatOverlappingTodayShift()
		{
			var date = new DateOnly(2012, 08, 28);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date.AddDays(-1), new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			addPersonSchedule(date, date.Date.AddHours(2).Utc(), date.Date.AddHours(8).Utc());

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(1);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(8);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvernightOvertimeAvailabilityWithTodayDayOff()
		{
			var date = new DateOnly(2012, 08, 28);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date.AddDays(-1), new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(LoggedOnUser.CurrentUser(), CurrentScenario.Current(), date, DayOffFactory.CreateDayOff(new Description("Dayoff")));
			ScheduleStorage.Add(dayOffAssignment);

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(0);

			result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void
			ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftStartingOnTheLastDayOfCurrentWeekOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var result = Target.GetDaySchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapAsmEnabledOnGetDayScheduleToTrueWhenHavingBothLicenseAndPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.AsmEnabled.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmEnabledOnGetDayScheduleToFalseWhenNoAsmLicense()
		{
			var date = new DateOnly(2012, 08, 26);
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.AsmEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldMapAsmEnabledOnGetDayScheduleToFalseWhenNoAsmPermission()
		{
			PermissionProvider.Enable();
			var date = new DateOnly(2012, 08, 26);
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.AsmEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldMapViewPossibilityPermissionOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.ViewPossibilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapRequestPermissionOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftTradeRequestPermissionOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.ShiftTradeRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapOvertimeRequestPermissionOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);

			result.OvertimeRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentDayOnGetDaySchedule()
		{
			var date = new DateOnly(2012, 08, 26);
			Now.Is(date.Date);

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetDaySchedule(date);
			result.IsCurrentDay.Should().Be.True();
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 13)
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenOvertimeRequestPeriod(overtimeRequestOpenDatePeriod);
			LoggedOnUser.CurrentUser().WorkflowControlSet = workFlowControlSet;
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

		private DateTime safeConvertTimeToUtc(DateOnly date)
		{
			return LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(date.Date);
		}

		private DateTime safeConvertTimeToUtc(DateTime date)
		{
			return safeConvertTimeToUtc(new DateOnly(date));
		}
	}
}