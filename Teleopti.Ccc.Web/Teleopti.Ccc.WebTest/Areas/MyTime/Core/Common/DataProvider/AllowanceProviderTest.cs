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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest 
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

			var skill=SkillFactory.CreateSkill("skill");

			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate, skill);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var scenario = new Scenario("thescenario");
			var budgetDays = new List<IBudgetDay>();
			const int allowance = 5;
			const double fulltimeEquivalentHours = 8d;
			var allowanceInMinutes = allowance*fulltimeEquivalentHours*60;
			var budgetDay = new BudgetDay(budgetGroup, scenario, period.StartDate) { Allowance = allowance, FulltimeEquivalentHours = fulltimeEquivalentHours };
			budgetDays.Add(budgetDay);

			user.AddPersonPeriod(personPeriod1);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(scenario);
			budgetDayRepository.Stub(x => x.Find(scenario, budgetGroup, period)).Return(budgetDays);
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var target = new AllowanceProvider(budgetDayRepository, loggedOnUser, scenarioRepository);
			var result = target.GetAllowanceForPeriod(period);
			result.First().Allowance.Should().Be.EqualTo(allowanceInMinutes);

		}
	}
}