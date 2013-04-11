using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest : PeopleAdminTestBase
	{
		[Test]
		public void ShouldGetAllowanceForPeriod()
		{
			var user = new Person();
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();

			CreateSkills();
			
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate, Skill1);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var scenario = new Scenario("thescenario");
			var budgetDays = new List<IBudgetDay>();
			const int allowance = 5;
			var budgetDay = new BudgetDay(budgetGroup, scenario, period.StartDate) {Allowance = allowance};
			budgetDays.Add(budgetDay);

			user.AddPersonPeriod(personPeriod1);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(scenario);
			budgetDayRepository.Stub(x => x.Find(scenario, budgetGroup, period)).Return(budgetDays);
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var target = new AllowanceProvider(budgetDayRepository, userTimeZone, loggedOnUser, scenarioRepository);
			var result = target.GetAllowanceForPeriod(period);
			result.First().Allowance.Should().Be.EqualTo(allowance);

		}
	}
}