using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture]
	public class ExtractBudgetGroupPeriodsTest
	{
		private IPerson _person;
		private ExtractBudgetGroupPeriods _target;

		[SetUp]
		public void Setup()
		{
			_person = PersonFactory.CreatePerson();
			_target = new ExtractBudgetGroupPeriods();

		}

		[Test]
		public void Extract_WhenOnlyOneAffectedPersonPeriod_ShouldReturnTheBudgetGroupForThatPersonPeriod()
		{
			var budgetGroup = new BudgetGroup();
			addPersonPeriod(new DateOnly(2001,1,1),budgetGroup);
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 10), new DateOnly(2001, 1, 20));

			var result = _target.BudgetGroupsForPeriod(_person, period).Single();
			
			Assert.That(result.Item2, Is.EqualTo(budgetGroup));
		}


		[Test]
		public void Extract_WhenOnlyOneAffectedPersonPeriod_ShouldReturnThePeriod()
		{
			addPersonPeriod(new DateOnly(2001,1,1));
			var period = new DateOnlyPeriod(new DateOnly(2001, 1, 10), new DateOnly(2001, 1, 20));

			var result = _target.BudgetGroupsForPeriod(_person, period).Single();

			Assert.That(result.Item1, Is.EqualTo(period));
		}

		
		[Test]
		public void Extract_WhenMultiplePersonPeriodsAreWithinTheAffectedPeriod_ShouldReturnAnItemForEachWithTheAffectedPeriod()
		{
			var start = new DateOnly(2001, 1, 10);
			var end = new DateOnly(2001, 1, 20);

			addPersonPeriod(new DateOnly(2001,1,1));
			addPersonPeriod(new DateOnly(2001,1,15));
			addPersonPeriod(new DateOnly(2001,1,18));

			var result = _target.BudgetGroupsForPeriod(_person, new DateOnlyPeriod(start, end)).ToList();
			Assert.That(result.Count(),Is.EqualTo(3));
			Assert.That(result[0].Item1, Is.EqualTo(new DateOnlyPeriod(start, new DateOnly(2001, 1, 14))), "First period");
			Assert.That(result[1].Item1, Is.EqualTo(new DateOnlyPeriod(new DateOnly(2001, 1, 15), new DateOnly(2001, 1, 17))), "Second period");
			Assert.That(result[2].Item1, Is.EqualTo(new DateOnlyPeriod(new DateOnly(2001, 1, 18), new DateOnly(2001, 1, 20))), "Third period");
		}

		[Test]
		public void Extract_WhenMultiplePersonPeriodsAreWithinTheAffectedPeriod_ShouldReturnAnItemForEachWithTheAffectedBudgetGroup()
		{
			var start = new DateOnly(2001, 1, 10);
			var end = new DateOnly(2001, 1, 20);


			var firstBudgetGroup = new BudgetGroup() { Name = "first" };
			var secondBudgetGroup = new BudgetGroup() { Name = "second" };
			var thirdBudgetGroup = new BudgetGroup() { Name = "third" };

			addPersonPeriod(new DateOnly(2001, 1, 1), firstBudgetGroup);
			addPersonPeriod(new DateOnly(2001, 1, 15), secondBudgetGroup);
			addPersonPeriod(new DateOnly(2001, 1, 18), thirdBudgetGroup);

			addPersonPeriod(new DateOnly(2001, 1, 1));
			addPersonPeriod(new DateOnly(2001, 1, 15));
			addPersonPeriod(new DateOnly(2001, 1, 18));

			var result = _target.BudgetGroupsForPeriod(_person, new DateOnlyPeriod(start, end)).ToList();
			Assert.That(result.Count(), Is.EqualTo(3));
			Assert.That(result[0].Item2, Is.EqualTo(firstBudgetGroup));
			Assert.That(result[1].Item2, Is.EqualTo(secondBudgetGroup));
			Assert.That(result[2].Item2, Is.EqualTo(thirdBudgetGroup));
		}

		

		#region helpers
		private void addPersonPeriod(DateOnly dateOnly)
		{
			_person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(dateOnly));
		}

		private void addPersonPeriod(DateOnly dateOnly, BudgetGroup budgetGroup)
		{
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(dateOnly);
			personPeriod.BudgetGroup = budgetGroup;
			_person.AddPersonPeriod(personPeriod);
		}

		#endregion
	}
}