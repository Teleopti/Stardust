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
	public class WeekScheduleDomainDataProviderWeekScheduleTest : IIsolateSystem
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeBudgetDayRepository BudgetDayRepository;
		public IScheduleStorage ScheduleStorage;
		public IWeekScheduleDomainDataProvider Target;
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
		public void ShouldMapDate()
		{
			var date = DateOnly.Today;

			var result = Target.GetWeekSchedule(date);
			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapAWeeksDays()
		{
			var date = DateOnly.Today;

			var result = Target.GetWeekSchedule(date);
			result.Days.Should().Have.Count.EqualTo(7);
		}

		[Test]
		public void ShouldMapWeeksDates()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var datesInWeek = (from d in firstDayOfWeek.Date.DateRange(7) select new DateOnly(d)).ToList();

			var result = Target.GetWeekSchedule(date);

			result.Days.Select(x => x.Date)
				.Should().Have.SameSequenceAs(datesInWeek);
		}

		[Test]
		public void ShouldMapScheduleDay()
		{
			var date = DateOnly.Today;
			var activity = new Activity("d");
			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(3), activity);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).ScheduleDay.PersonAssignment().Period.StartDateTime.Should().Be(DateTime.SpecifyKind(date.Date.Add(TimeSpan.FromHours(1)), DateTimeKind.Utc));
		}

		[Test]
		public void ShouldMapProjection()
		{
			var date = DateOnly.Today;
			var activity = new Activity("d").WithId();

			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(3), activity);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).Projection.ElementAt(0).Payload.Id.Value.Should().Be(activity.Id);
		}

		[Test]
		public void ShouldMapProjectionIncludingTheDayBeforeCurrentWeek()
		{
			var date = new DateOnly(2012, 08, 27);
			var yesterdayDate = new DateOnly(2012, 08, 26);
			var activityYesterday = new Activity("d").WithId();
			var activityToday = new Activity("d").WithId();
			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(3), activityToday);
			addPersonSchedule(yesterdayDate, TimeSpan.FromHours(1), TimeSpan.FromHours(3), activityYesterday);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).ProjectionYesterday.ElementAt(0).Payload.Id.Value.Should().Be(activityYesterday.Id);
		}

		[Test]
		public void ShouldMapOvertimeAvailability()
		{
			var date = DateOnly.Today;
			var overtimeAvailability = new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)).WithId();
			ScheduleStorage.Add(overtimeAvailability);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailability.Should().Be(overtimeAvailability);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityForYesterday()
		{
			var date = new DateOnly(2012, 08, 27);
			var yesterdayDate = new DateOnly(2012, 08, 26);
			var overtimeAvailability = new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)).WithId();
			ScheduleStorage.Add(overtimeAvailability);
			var overtimeAvailabilityYesterday = new OvertimeAvailability(LoggedOnUser.CurrentUser(), yesterdayDate, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)).WithId();
			ScheduleStorage.Add(overtimeAvailabilityYesterday);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailabilityYesterday.Should().Be(overtimeAvailabilityYesterday);
		}

		[Test]
		public void ShouldMapPersonRequests()
		{
			var date = new DateOnly(DateTime.UtcNow.Date);

			var personRequest = new PersonRequest(new Person(),
												  new TextRequest(
													new DateTimePeriod(
														DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
													));
			PersonRequestRepository.Add(personRequest);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).PersonRequestCount.Should().Be(1);
		}

		[Test]
		public void ShouldMapClass()
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

			var result = Target.GetWeekSchedule(date);
			result.Days.Single(d => d.Date == lastDayOfWeek).ProbabilityClass.Should().Be.EqualTo(BudgetCssClass.Poor);
		}

		[Test]
		public void ShouldMapText()
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

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == lastDayOfWeek).ProbabilityText.Should().Be.EqualTo("Poor");
		}

		[Test]
		public void ShouldMapAvailability()
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

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == lastDayOfWeek).Availability.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMapPersonRequestsStartingAtMidnight()
		{
			var date = DateOnly.Today;

			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);

			var personRequest = new PersonRequest(new Person(),
												  new TextRequest(
													new DateTimePeriod(
														localMidnightInUtc, localMidnightInUtc.AddHours(1))
													));
			PersonRequestRepository.Add(personRequest);

			var result = Target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).PersonRequestCount.Should().Be(1);
		}

		[Test]
		public void ShouldMapColorSource()
		{
			var date = DateOnly.Today;
			var activity = new Activity("d").WithId();
			addPersonSchedule(date, TimeSpan.FromHours(1), TimeSpan.FromHours(3), activity);

			var result = Target.GetWeekSchedule(date);

			result.ColorSource.ScheduleDays.FirstOrDefault(s => s.DateOnlyAsPeriod.DateOnly == date).PersonAssignment()
				.ShiftLayers.FirstOrDefault().Payload.Should().Be(activity);

			var projection = result.ColorSource.Projections.FirstOrDefault(p => p.Period() != null && p.Period().Value.StartDateTime.Date == date.Date);
			projection.Period().GetValueOrDefault().StartDateTime.Should().Be(date.Date.Add(TimeSpan.FromHours(1)));
		}

		[Test]
		public void ShouldMapMinMaxTime()
		{
			var date = DateOnly.Today;
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(8), localMidnightInUtc.AddHours(17), new Activity().WithId());

			var result = Target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(7);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(17);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShift()
		{
			var date = new DateOnly(2012, 08, 28);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28), new Activity().WithId());

			var result = Target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldHaveCorrectStartTimeForEndingAtMidNightEdgeCase()
		{
			var date = new DateOnly(2012, 08, 28);
			var localMidnightInUtc = safeConvertTimeToUtc(date.Date);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(24), new Activity().WithId());

			var result = Target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShiftFromPreviousWeek()
		{
			var date = new DateOnly(2012, 08, 28);
			Now.Is(date.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var localMidnightInUtc = safeConvertTimeToUtc(firstDayOfWeek.AddDays(-1).Date);
			addPersonSchedule(firstDayOfWeek.AddDays(-1), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28), new Activity().WithId());

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShiftStartingOnTheLastDayOfCurrentWeek()
		{
			var date = new DateOnly(2012, 08, 26);
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(lastDayOfWeek);
			addPersonSchedule(date, localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetWeekSchedule(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailability()
		{
			var date = new DateOnly(2013, 09, 11);
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)));

			var result = Target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(2);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShift()
		{
			var date = new DateOnly(2013, 09, 11);
			addPersonSchedule(date, TimeSpan.Zero, TimeSpan.FromDays(1));
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var result = Target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(0);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftFromPreviousWeek()
		{
			var date = new DateOnly(2012, 08, 28);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
		
			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), firstDayOfWeek.AddDays(-1), new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftStartingOnTheLastDayOfCurrentWeek()
		{
			var date = new DateOnly(2012, 08, 26);
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			ScheduleStorage.Add(new OvertimeAvailability(LoggedOnUser.CurrentUser(), lastDayOfWeek, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0)));

			var result = Target.GetWeekSchedule(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapAsmEnabledToTrueWhenHavingBothLicenseAndPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.AsmEnabled.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmLicense()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);
			
			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.AsmEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmPermission()
		{
			PermissionProvider.Enable();
			
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.AsmEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldMapViewPossibilityPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));
			
			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.ViewPossibilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapRequestPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));
			
			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapOvertimeRequestPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.OvertimeRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentWeek()
		{
			var date = new DateTime(2014, 08, 26);
			Now.Is(date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));

			var localMidnightInUtc = safeConvertTimeToUtc(new DateOnly(lastDayOfWeek.Date));
			addPersonSchedule(new DateOnly(lastDayOfWeek.Date), localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var result = Target.GetWeekSchedule(firstDayOfWeek);

			result.IsCurrentWeek.Should().Be.True();
		}

		private void addPersonSchedule(DateOnly date, TimeSpan startTime, TimeSpan endTime, IActivity activity = null)
		{
			var start = DateTime.SpecifyKind(date.Date.Add(startTime), DateTimeKind.Utc);
			var end = DateTime.SpecifyKind(date.Date.Add(endTime), DateTimeKind.Utc);
			addPersonSchedule(date, start, end, activity);
		}

		private void addPersonSchedule(DateOnly date, DateTime startTime, DateTime endTime, IActivity activity =  null)
		{
			activity = activity ?? new Activity().WithId();
			var assignmentPeriod = new DateTimePeriod(startTime, endTime);
			var personAssignment = new PersonAssignment(LoggedOnUser.CurrentUser(), CurrentScenario.Current(), date).WithId();
			personAssignment.AddActivity(activity, assignmentPeriod);
			ScheduleStorage.Add(personAssignment);
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