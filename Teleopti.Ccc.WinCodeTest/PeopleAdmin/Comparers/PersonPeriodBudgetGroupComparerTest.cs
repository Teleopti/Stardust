using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	[TestFixture]
	public class PersonPeriodBudgetGroupComparerTest
	{
		private IPerson _person, _person1;
		private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3, _personPeriod4;
		private PersonPeriodBudgetGroupComparer _personPeriodBudgetGroupComparer;
		private int result;

		private DateOnly _universalTime1 = new DateOnly(2058, 10, 09);
		private DateOnly _universalTime2 = new DateOnly(2028, 10, 09);
		private DateOnly _universalTime3 = new DateOnly(2008, 10, 09);

		[SetUp]
		public void Setup()
		{
			_person = PersonFactory.CreatePerson("Test 1");

			BudgetGroup budgetGroup1 = new BudgetGroup { Name = "BG A" };
			BudgetGroup budgetGroup2 = new BudgetGroup { Name = "BG C" };

			var team = TeamFactory.CreateSimpleTeam();

			_personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, team, budgetGroup1);
			_personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, team, budgetGroup2);

			_person.AddPersonPeriod(_personPeriod1);
			_person.AddPersonPeriod(_personPeriod2);

			_person1 = PersonFactory.CreatePerson("Test 2");

			BudgetGroup budgetGroup3 = new BudgetGroup { Name = "BG A" };
			BudgetGroup budgetGroup4 = new BudgetGroup { Name = "BG B" };

			_personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, team, budgetGroup3);
			_personPeriod4 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, team, budgetGroup4);

			_person1.AddPersonPeriod(_personPeriod3);
			_person1.AddPersonPeriod(_personPeriod4);
		}

		[TearDown]
		public void TestDispose()
		{
			_target = null;
			_personPeriodModel = null;

			_personPeriod1 = null;
			_personPeriod2 = null;
			_personPeriod3 = null;

			_personPeriodBudgetGroupComparer = null;
		}

		[Test]
		public void VerifyCompareMethodWithAllNull()
		{
            _target = new PersonPeriodModel(_universalTime3, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime3, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(0, result);
		}

		[Test]
		public void VerifyCompareMethodWithFirstNull()
		{
            _target = new PersonPeriodModel(_universalTime3, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime1, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(-1, result);
		}

		[Test]
		public void VerifyCompareMethodWithSecondNull()
		{
            _target = new PersonPeriodModel(_universalTime1, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime3, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(1, result);
		}

		[Test]
		public void VerifyCompareMethodAscending()
		{
            _target = new PersonPeriodModel(_universalTime1, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime2, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(-1, result);
		}

		[Test]
		public void VerifyCompareMethodDescending()
		{
            _target = new PersonPeriodModel(_universalTime2, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime1, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(1, result);
		}

		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
            _target = new PersonPeriodModel(_universalTime1, _person, new List<IPersonSkill>(), null, null, null);
            _personPeriodModel = new PersonPeriodModel(_universalTime1, _person1, new List<IPersonSkill>(), null, null, null);

			_personPeriodBudgetGroupComparer = new PersonPeriodBudgetGroupComparer();
			result = _personPeriodBudgetGroupComparer.Compare(_target, _personPeriodModel);

			Assert.AreEqual(0, result);
		}
	}
}
