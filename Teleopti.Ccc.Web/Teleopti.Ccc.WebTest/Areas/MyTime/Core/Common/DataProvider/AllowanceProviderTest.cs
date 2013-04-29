using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest 
	{
		private Scenario _scenario;
		private IScenarioRepository _scenarioRepository;
		private ILoggedOnUser _loggedOnUser;
		private IPerson _user;

		[SetUp]
		public void Setup()
		{
			_scenario = new Scenario("default");
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_scenarioRepository.Expect(s => s.LoadDefaultScenario()).Repeat.Any().Return(_scenario);

			_user = PersonFactory.CreatePerson("some person");
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Expect(l => l.CurrentUser()).Repeat.Any().Return(_user);
		}

		[Test]
		public void GetAllowanceForPeriod_WhenNoBudgetDays_ShouldSetAllowanceToZeroOnEverydayInThePeriod()
		{
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(new DateOnly(2001,1,1),new DateOnly(2001,1,10));

			budgetDayRepository.Stub(x => x.Find(_scenario, null, period)).Return(new List<IBudgetDay>());

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository, new ExtractBudgetGroupPeriods());
			var result = target.GetAllowanceForPeriod(period).ToList();

			foreach (var date in period.DayCollection())
			{
				var allowanceDay = result.Single(r => r.Date == date);
				Assert.That(allowanceDay.Allowance,Is.EqualTo(0));
			}
		}

		[Test]
		public void GetAllowanceForPeriod_WhenBudgetDayExistWitthinThePeriod_ShouldSetTheAllowanceInMinutesForThatDate()
		{
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var budgetDays = new List<IBudgetDay>();
			var allowance = TimeSpan.FromHours(40);

			var budgetDay = createBudgetDayWithAllowance(budgetGroup, period.StartDate, allowance.TotalHours);
			budgetDays.Add(budgetDay);

			_user.AddPersonPeriod(personPeriod1);
			budgetDayRepository.Stub(x => x.Find(_scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AllowanceProvider(budgetDayRepository, _loggedOnUser, _scenarioRepository, new ExtractBudgetGroupPeriods());
			var result = target.GetAllowanceForPeriod(period);
			result.First().Allowance.Should().Be.EqualTo(allowance.TotalMinutes);
		}

		[Test]
		public void
		GetAllowanceForPeriod_WhenThereAreMultiplePersonPeriodWithDifferentBudgetGroupssWithinThatPeriod_ShouldUseAllBudgetGroupsToFindTheBudgetDays()
		{
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 10));
			var budgetDayRepo = MockRepository.GenerateMock<IBudgetDayRepository>();
			var extractBudgetGroupPeriods = MockRepository.GenerateMock<IExtractBudgetGroupPeriods>();

			var budgetGroup1 = new BudgetGroup();
			var period1 = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 5));
			var day1 = period1.StartDate;
			var allowance1 = TimeSpan.FromHours(10);
			var budgetDay1 = createBudgetDayWithAllowance(budgetGroup1, day1, allowance1.TotalHours);

			var budgetGroup2 = new BudgetGroup();
			var period2 = new DateOnlyPeriod(new DateOnly(2001, 1, 6), new DateOnly(2001, 1, 10));
			var day2 = period2.StartDate;
			var allowance2 = TimeSpan.FromHours(8);
			var budgetDay2 = createBudgetDayWithAllowance(budgetGroup2, day2, allowance2.TotalHours);


			extractBudgetGroupPeriods.Expect(e => e.BudgetGroupsForPeriod(_user, period))
			                         .Return(new List<Tuple<DateOnlyPeriod, IBudgetGroup>>
				                                 {
					                                 new Tuple<DateOnlyPeriod, IBudgetGroup>(period1,budgetGroup1),
													 new Tuple<DateOnlyPeriod, IBudgetGroup>(period2,budgetGroup2)
				                                 });

			budgetDayRepo.Expect(b => b.Find(_scenario, budgetGroup1, period1)).Return(new List<IBudgetDay> {budgetDay1});
			budgetDayRepo.Expect(b => b.Find(_scenario, budgetGroup2, period2)).Return(new List<IBudgetDay> {budgetDay2});
			var target = new AllowanceProvider(budgetDayRepo, _loggedOnUser, _scenarioRepository, extractBudgetGroupPeriods);

			var result = target.GetAllowanceForPeriod(period).ToList();

			budgetDayRepo.VerifyAllExpectations();

			var stuff = result.FirstOrDefault(d => d.Date.Equals(day1));

			//Assert.That(result.Where(d => d.Date == day1).Sum(d => d.Allowance), Is.EqualTo(allowance1.TotalMinutes));
			//Assert.That(result.Where(d => d.Date == day2).Sum(d => d.Allowance), Is.EqualTo(allowance2.TotalMinutes));

			Assert.That(result.Single(d=>d.Date==day1).Allowance,Is.EqualTo(allowance1.TotalMinutes));
			Assert.That(result.Single(d=>d.Date==day2).Allowance,Is.EqualTo(allowance2.TotalMinutes));
		}

		private BudgetDay createBudgetDayWithAllowance(IBudgetGroup budgetGroup,DateOnly date, double hoursOfAllowance)
		{
			return new BudgetDay(budgetGroup, _scenario, date) {FulltimeEquivalentHours = 1, Allowance = hoursOfAllowance};
		}	
	}
}