using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture, IoCTest]
	public class AbsenceRequestProbabilityProviderNoMockTest : IIsolateSystem
	{
		public IBudgetDayRepository BudgetDayRepository;
		public ILoggedOnUser LoggedOnUser;
		public IScenarioRepository ScenarioRepository;
		public IExtractBudgetGroupPeriods ExtractBudgetGroupPeriods;
		public INow Now;
		public FakeScheduleProjectionReadOnlyPersister ScheduleProjectionReadOnlyPersister;
		public IAbsenceTimeProviderCache AbsenceTimeProviderCache;

		private IScenario _scenario;
		private IAbsence _absence;
		private readonly DateTime _today = new DateTime(2016, 10, 25, 0, 0, 0, DateTimeKind.Utc);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			isolate.UseTestDouble<ExtractBudgetGroupPeriods>().For<IExtractBudgetGroupPeriods>();
			isolate.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			isolate.UseTestDouble<AbsenceTimeProviderCache>().For<IAbsenceTimeProviderCache>();
			isolate.UseTestDouble(new MutableNow(_today)).For<INow>();

			_absence = AbsenceFactory.CreateAbsence("holiday").WithId();
		}

		[Test]
		public void ShouldOnlySetCssClassInAbsenceRequestProbabilityForBudgetGroupHeadCountAbsencePeriod()
		{
			setupCommonData();

			addRollingAbsenceRequestPeriod(new MinMax<int>(1, 2), new StaffingThresholdValidator());
			addRollingAbsenceRequestPeriod(new MinMax<int>(3, 4), new BudgetGroupHeadCountValidator());

			var period = new DateOnlyPeriod(2016, 10, 26, 2016, 10, 29);
			var absenceRequestProbabilities = getAbsenceRequestProbabilityForPeriod(period);

			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 26), string.Empty, string.Empty);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 27), string.Empty, string.Empty);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 28), "yellow", UserTexts.Resources.Fair);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 29), "yellow", UserTexts.Resources.Fair);
		}

		[Test]
		public void ShouldOnlySetCssClassInAbsenceRequestProbabilityForBudgetGroupHeadCountAllowanceAbsencePeriod()
		{
			setupCommonData();

			addRollingAbsenceRequestPeriod(new MinMax<int>(1, 2), new StaffingThresholdValidator());
			addRollingAbsenceRequestPeriod(new MinMax<int>(3, 4), new BudgetGroupAllowanceValidator());

			var period = new DateOnlyPeriod(2016, 10, 26, 2016, 10, 29);
			var absenceRequestProbabilities = getAbsenceRequestProbabilityForPeriod(period);

			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 26), string.Empty, string.Empty);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 27), string.Empty, string.Empty);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 28), "red", UserTexts.Resources.Poor);
			assertAbsenceRequestProbability(absenceRequestProbabilities, new DateOnly(2016, 10, 29), "red", UserTexts.Resources.Poor);
		}

		private void setupCommonData()
		{
			_scenario = ScenarioFactory.CreateScenario("default", true, true).WithId();
			ScenarioRepository.Add(_scenario);

			var date = new DateOnly(_today);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(date);
			personPeriod.BudgetGroup = new BudgetGroup();
			person.AddPersonPeriod(personPeriod);
			person.WorkflowControlSet = new WorkflowControlSet();
		}

		private void addRollingAbsenceRequestPeriod(MinMax<int> betweenDays, IAbsenceRequestValidator validator)
		{
			var workflowControlSet = LoggedOnUser.CurrentUser().WorkflowControlSet;
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = _absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = betweenDays,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 1, 1, 2016, 12, 31),
				StaffingThresholdValidator = validator
			});
		}

		private List<IAbsenceRequestProbability> getAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period)
		{
			addReadModelActivity(period);
			addBudgetDays(period);

			var allowanceProvider = new AllowanceProvider(BudgetDayRepository, LoggedOnUser, ScenarioRepository,
				ExtractBudgetGroupPeriods, Now);
			var absenceTimeProvider = new AbsenceTimeProvider(LoggedOnUser, ScenarioRepository, ScheduleProjectionReadOnlyPersister,
				ExtractBudgetGroupPeriods, AbsenceTimeProviderCache);
			var provider = new AbsenceRequestProbabilityProvider(allowanceProvider, absenceTimeProvider, Now);
			var absenceRequestProbabilities = provider.GetAbsenceRequestProbabilityForPeriod(period);
			return absenceRequestProbabilities;
		}

		private void addReadModelActivity(DateOnlyPeriod period)
		{
			var days = period.DayCollection();
			foreach (var day in days)
			{
				ScheduleProjectionReadOnlyPersister.AddActivity(new ScheduleProjectionReadOnlyModel
				{
					ScenarioId = _scenario.Id.GetValueOrDefault(),
					StartDateTime = day.Date,
					EndDateTime = day.Date,
					ContractTime = TimeSpan.FromMinutes(480),
					BelongsToDate = day
				});
			}
		}

		private void addBudgetDays(DateOnlyPeriod period)
		{
			var personPeriod = LoggedOnUser.CurrentUser().Period(new DateOnly(_today));
			var days = period.DayCollection();
			foreach (var day in days)
			{
				var budgetDay = new BudgetDay(personPeriod.BudgetGroup, _scenario, day)
				{
					ShrinkedAllowance = 1,
					FulltimeEquivalentHours = 8,
					AbsenceOverride = 1
				};
				BudgetDayRepository.Add(budgetDay);
			}
		}

		private void assertAbsenceRequestProbability(IEnumerable<IAbsenceRequestProbability> absenceRequestProbabilities,
			DateOnly date
			, string cssClass, string text)
		{
			var absenceRequestProbability = absenceRequestProbabilities.FirstOrDefault(a => a.Date == date);
			absenceRequestProbability.Should().Not.Be(null);
			absenceRequestProbability.CssClass.Should().Be(cssClass);
			absenceRequestProbability.Text.Should().Be(text);
		}
	}
}