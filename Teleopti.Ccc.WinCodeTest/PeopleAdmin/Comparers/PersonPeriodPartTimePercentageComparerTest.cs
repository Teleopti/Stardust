using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Tests for part time percentage
	/// </summary>
	[TestFixture]
	public class PersonPeriodPartTimePercentageComparerTest
	{
	    private IPerson _person, _person1;
		private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private readonly List<IPersonSkill> _personSkillCollection = new List<IPersonSkill>();
		private readonly List<IPersonSkill> _personSkillCollection1 = new List<IPersonSkill>();
		private PersonPeriodPartTimePercentageComparer personPeriodPartTimePercentageComparer;
		private int result;

        private DateOnly _universalTime1 = new DateOnly(2058, 10, 09);
        private DateOnly _universalTime2 = new DateOnly(2028, 10, 09);
        private DateOnly _universalTime3 = new DateOnly(2008, 10, 09);

	    /// <summary>
		/// Tests the init.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 24-07-2008
		/// </remarks>
		[SetUp]
		public void Setup()
		{
	        _person = GetPerson("Test 1", _universalTime1, "Test A", _universalTime2, "Test C");
    
            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill1", 1));
            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill2", 1));
            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill3", 1));

	        _person1 = GetPerson("Test 2", _universalTime1, "Test A", _universalTime2, "Test B");

            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill4", 1));
            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill5", 1));
            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill6", 1));
		}

		/// <summary>
		/// Tests the dispose.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 23-07-2008
		/// </remarks>
		[TearDown]
		public void TestDispose()
		{
			// Sets teh objects to null
			_target = null;
			_personPeriodModel = null;

			personPeriodPartTimePercentageComparer = null;
		}

	    private static IPerson GetPerson(string personName, DateOnly firstperiodDate, string firstPartTimePercentageName,
            DateOnly secondperiodDate, string secondPartTimePercentageName)
        {
            IPerson person = PersonFactory.CreatePerson(personName);

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod
                (firstperiodDate,
                 PersonContractFactory.CreatePersonContract("my first contract", "Testing 1", firstPartTimePercentageName),
                 new Team());

            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod
                (secondperiodDate,
                 PersonContractFactory.CreatePersonContract("my first contract", "Test 1", secondPartTimePercentageName),
                 new Team());

            IList<IPersonPeriod> periodList = new List<IPersonPeriod>();
            periodList.Add(personPeriod1);
            periodList.Add(personPeriod2);

            return PersonFactory.GetPerson(person, periodList);
        }

	    /// <summary>
		/// Verifies the compare method with null values for all parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 24-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithAllNull()
		{
			// Sets the current part time percentage
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 24-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithFirstNull()
		{
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 23-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondNull()
		{
			// Sets the current part time percentage
            _target = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person1, _personSkillCollection1, null, null, null);
		
			_personPeriodModel.PartTimePercentage = null;

			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 23-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodAscending()
		{
			// Sets the current part time percentage
            _target = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime2.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for teh second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodDescending()
		{
            _target = new PersonPeriodModel(new DateOnly(_universalTime2.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with same role for both parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
            _target = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person1, _personSkillCollection1, null, null, null);
		
			// Calls the compares method
			personPeriodPartTimePercentageComparer = new PersonPeriodPartTimePercentageComparer();
			result = personPeriodPartTimePercentageComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
