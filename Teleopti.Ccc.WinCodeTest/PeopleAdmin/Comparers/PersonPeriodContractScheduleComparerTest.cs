using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PersonPeriodContractComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 24-07-2008
	/// </remarks>
	[TestFixture]
	public class PersonPeriodContractScheduleComparerTest
	{
	    private IPerson _person, _person1;
		private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private readonly List<IPersonSkill> _personSkillCollection = new List<IPersonSkill>();
		private readonly List<IPersonSkill> _personSkillCollection1 = new List<IPersonSkill>();
		private PersonPeriodContractScheduleComparer personPeriodContractScheduleComparer;
		private int result;

        private DateOnly date1 = new DateOnly(2020, 01, 01);
        private DateOnly date2 = new DateOnly(2030, 01, 01);
        private DateOnly date3 = new DateOnly(2010, 01, 01);

        [SetUp]
        public void Setup()
		{
	        _person = GetPerson("Test 1", date2, "Test A", date1, "Test C");

            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill1", 1));
            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill2", 1));
            _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill3", 1));

	        _person1 = GetPerson("Test 2", date2, "Test A", date1, "Test B");

            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill4", 1));
            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill5", 1));
            _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill6", 1));
		}

		[TearDown]
		public void TestDispose()
		{
			// Sets teh objects to null
			_target = null;
			_personPeriodModel = null;

			personPeriodContractScheduleComparer = null;
		}

	    private static IPerson GetPerson(string personName, DateOnly firstperiodDate, string firstContractScheduleName,
            DateOnly secondperiodDate, string secondContractScheduleName)
        {
            IPerson person = PersonFactory.CreatePerson(personName);

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod
                (firstperiodDate,
                 PersonContractFactory.CreatePersonContract("my first contract", firstContractScheduleName, "Testing"),
                 new Team());

            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod
                (secondperiodDate,
                 PersonContractFactory.CreatePersonContract("my first contract", secondContractScheduleName, "Testing"),
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
            _target = new PersonPeriodModel(date3, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date3, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(date3, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date2, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(date2, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date3, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);


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
            _target = new PersonPeriodModel(date2, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date1, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(date1, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date2, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(date2, _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(date2, _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			personPeriodContractScheduleComparer = new PersonPeriodContractScheduleComparer();
			result = personPeriodContractScheduleComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
