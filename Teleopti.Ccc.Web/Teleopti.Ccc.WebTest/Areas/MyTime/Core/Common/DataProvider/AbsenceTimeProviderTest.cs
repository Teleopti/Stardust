using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AbsenceTimeProviderTest 
	{
		[Test,Ignore]
		public void ShouldGetAbsenceTimeForPeriod()
		{
			var user = new Person();
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			var skill = SkillFactory.CreateSkill("_skill1");

			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(period.StartDate, skill);

			var budgetGroup = new BudgetGroup();
			personPeriod1.BudgetGroup = budgetGroup;

			var scenario = new Scenario("thescenario");
			var budgetDays = new List<IBudgetDay>();
			const int allowance = 5;
			var budgetDay = new BudgetDay(budgetGroup, scenario, period.StartDate) { Allowance = allowance };
			budgetDays.Add(budgetDay);

			user.AddPersonPeriod(personPeriod1);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(scenario);
			budgetDayRepository.Stub(x => x.Find(scenario, budgetGroup, period)).Return(budgetDays);

			var target = new AbsenceTimeProvider(budgetDayRepository, loggedOnUser, scenarioRepository, scheduleProjectionReadOnlyRepository);
			var result = target.GetAbsenceTimeForPeriod(period);
			result.First().AbsenceTime.Should().Be.EqualTo(allowance);
		}

	
	}
}