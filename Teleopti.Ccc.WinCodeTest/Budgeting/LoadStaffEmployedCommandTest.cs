using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class LoadStaffEmployedCommandTest
	{
		private MockRepository mocks;
		private IBudgetGroup budgetGroup;
		private IBudgetDay budgetDay2;
		private IBudgetDay budgetDay1;
		private BudgetGroupDayDetailModel dayDetail1;
		private BudgetGroupDayDetailModel dayDetail2;
		private BudgetGroupMainModel mainModel;
		private IScenario scenario;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();

			budgetGroup = new BudgetGroup();
			
			budgetGroup.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			scenario = ScenarioFactory.CreateScenarioAggregate();

			budgetDay1 = new BudgetDay(budgetGroup, scenario, new DateOnly(2010, 10, 20));
			budgetDay2 = new BudgetDay(budgetGroup, scenario, new DateOnly(2010, 10, 22));

			dayDetail1 = new BudgetGroupDayDetailModel(budgetDay1);
			dayDetail2 = new BudgetGroupDayDetailModel(budgetDay2);

		    mainModel = new BudgetGroupMainModel(null)
		                    {
		                        BudgetGroup = budgetGroup,
		                        Period = new DateOnlyPeriod(2010, 10, 15, 2010, 10, 31),
		                        Scenario = scenario
		                    };
		}

		[Test]
		public void ShouldLoadStaffEmployedForTheGivenBudgetDay()
		{
			IEnumerable<IPerson> people = GetPeopleWithPartTimePercentagesSet();

		    var budgetDaySource = mocks.StrictMock<IBudgetDaySource>();
            var budgetPeopleProvider = mocks.StrictMock<IBudgetPeopleProvider>();
			using (mocks.Record())
			{
				Expect.Call(budgetDaySource.Find()).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });
				Expect.Call(budgetPeopleProvider.FindPeopleWithBudgetGroup(budgetGroup, budgetDay1.Day)).Return(people);
				Expect.Call(budgetDaySource.GetFulltimeEquivalentHoursPerDay(dayDetail1.FulltimeEquivalentHours)).Return(8d);
			}
			using (mocks.Playback())
			{
				var target = new LoadStaffEmployedCommand(budgetPeopleProvider, mainModel, budgetDaySource);
				target.Execute();
				Assert.AreEqual(1.5d, dayDetail1.StaffEmployed);
				Assert.IsNull(dayDetail2.StaffEmployed);
			}
		}

	    private static IEnumerable<IPerson> GetPeopleWithPartTimePercentagesSet()
	    {
	        var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 10, 15), new List<ISkill>());
	        var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 10, 20), new List<ISkill>());

	        var parttimePercentage1 = PartTimePercentageFactory.CreatePartTimePercentage("50%");
	        parttimePercentage1.Percentage = new Percent(0.5d);

	        var parttimePercentage2 = PartTimePercentageFactory.CreatePartTimePercentage("100%");
	        parttimePercentage2.Percentage = new Percent(1d);

	        var contract = ContractFactory.CreateContract("Test C1");
	        contract.WorkTime = new WorkTime(TimeSpan.FromHours(8d));

	        var contractSchedule = ContractScheduleFactory.CreateContractSchedule("Test CS1");
	        person1.PersonPeriodCollection[0].PersonContract = PersonContractFactory.CreatePersonContract(contract, parttimePercentage1, contractSchedule);
	        person2.PersonPeriodCollection[0].PersonContract = PersonContractFactory.CreatePersonContract(contract, parttimePercentage2, contractSchedule);

	        return new List<IPerson> { person1, person2 };
	    }

	    [Test]
		public void ShouldNotLoadStaffEmployedForTheGivenBudgetDayIfNoFulltimeEquivalentHoursSpecified()
		{
			var budgetDaySource = mocks.StrictMock<IBudgetDaySource>();
			
			var budgetPeopleProvider = mocks.StrictMock<IBudgetPeopleProvider>();
			using (mocks.Record())
			{
				Expect.Call(budgetDaySource.Find()).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });
				Expect.Call(budgetDaySource.GetFulltimeEquivalentHoursPerDay(dayDetail1.FulltimeEquivalentHours)).Return(null);
			}
			using (mocks.Playback())
			{
				var target = new LoadStaffEmployedCommand(budgetPeopleProvider, mainModel, budgetDaySource);
				target.Execute();
				Assert.IsNull(dayDetail1.StaffEmployed);
				Assert.IsNull(dayDetail2.StaffEmployed);
			}
		}

		[Test]
		public void ShouldNotLoadStaffEmployedForTheGivenBudgetDayIfZeroFulltimeEquivalentHoursSpecified()
		{
			var budgetDaySource = mocks.StrictMock<IBudgetDaySource>();

			var budgetPeopleProvider = mocks.StrictMock<IBudgetPeopleProvider>();
			using (mocks.Record())
			{
				Expect.Call(budgetDaySource.Find()).Return(new List<IBudgetGroupDayDetailModel> { dayDetail1, dayDetail2 });
				Expect.Call(budgetDaySource.GetFulltimeEquivalentHoursPerDay(dayDetail1.FulltimeEquivalentHours)).Return(0);
			}
			using (mocks.Playback())
			{
				var target = new LoadStaffEmployedCommand(budgetPeopleProvider, mainModel, budgetDaySource);
				target.Execute();
				Assert.IsNull(dayDetail1.StaffEmployed);
				Assert.IsNull(dayDetail2.StaffEmployed);
			}
		}
		[Test]
		public void ShouldNotLoadStaffEmployedIfNoSelectedBudgetDay()
		{
			var budgetDaySource = mocks.StrictMock<IBudgetDaySource>();
			var budgetPeopleProvider = mocks.StrictMock<IBudgetPeopleProvider>();
			using (mocks.Record())
			{
				Expect.Call(budgetDaySource.Find()).Return(new List<IBudgetGroupDayDetailModel>());
			}
			using (mocks.Playback())
			{
				var target = new LoadStaffEmployedCommand(budgetPeopleProvider, mainModel, budgetDaySource);
				target.Execute();
				Assert.IsNull(dayDetail1.StaffEmployed);
				Assert.IsNull(dayDetail2.StaffEmployed);
			}
		}

		[Test]
		public void ShouldGetPeopleWithGivenBudgetGroup()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 10, 15), new List<ISkill>());
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 10, 20), new List<ISkill>());

			person1.PersonPeriodCollection[0].BudgetGroup = budgetGroup;
			person2.PersonPeriodCollection[0].BudgetGroup = budgetGroup;

			var people = new List<IPerson> { person1, person2 };

			var personRepository = mocks.StrictMock<IPersonRepository>();
			using (mocks.Record())
			{
				var periodForQuery = new DateOnlyPeriod(budgetDay1.Day.AddDays(-1), budgetDay1.Day.AddDays(1));
				Expect.Call(personRepository.FindAllAgentsLight(periodForQuery)).Return(people);
			}
			using (mocks.Playback())
			{
				var target = new BudgetPeopleProvider(personRepository);
				var result = target.FindPeopleWithBudgetGroup(budgetGroup,budgetDay1.Day);
				Assert.AreEqual(2, result.Count());
			}
		}
	}
}
