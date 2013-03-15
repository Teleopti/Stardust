using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest : PeopleAdminTestBase
	{
		private IPersonPeriod _personPeriod1;
		private BudgetGroup _budgetGroup;
		private IPerson _person = PersonFactory.CreatePerson("Ash", "Andeen");
		
		[SetUp]
		public void Setup()
		{

			CreateSkills();

			CreatePersonSkillCollection();

			CreateExternalLogOnCollection();

			CreateSiteTeamCollection();

			_personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly1,
																			  Skill1);
			_person.PersonPeriodCollection.Add(_personPeriod1);
		}

		[Test]
		public void ShouldGetAllowance()
		{
			//var person = MockRepository.GenerateMock<IPerson>();
			_budgetGroup = new BudgetGroup();
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
			var budgetGroupRepository = MockRepository.GenerateMock<IBudgetGroupRepository>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();

			//loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			budgetGroupRepository.Stub(x => x.LoadAll()).Return(new List<IBudgetGroup> { _budgetGroup });
			budgetDayRepository.Stub(x => x.Find(null, null, new DateOnlyPeriod())).IgnoreArguments().Return(new List<IBudgetDay>());

			_personPeriod1.BudgetGroup = _budgetGroup;

			var target = new AllowanceProvider(budgetDayRepository, userTimeZone, loggedOnUser, scenarioRepository);

			var result = target.GetAllowanceForPeriod(period);

			result.First().Date.Should().Be.EqualTo(period.StartDate.Date);

		}


		[Test]
		public void VerifyCurrentBudgetGroupCanSet()
		{
			BudgetGroup budgetGroup = new BudgetGroup();
			_target.BudgetGroup = budgetGroup;

			Assert.AreEqual(budgetGroup, _target.BudgetGroup);

		}






	}
}