using System;
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
	public class PersonPeriodPersonSkillComparerTest
	{
		#region Variables

		private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3;
		private PersonPeriodPersonSkillComparer personPeriodPersonSkillComparer;
		private int result;

        private DateOnly _from3 = new DateOnly(2008, 10, 09);
		#endregion

		#region SetUp and TearDown

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
		    IPerson person = GetPerson("Test 1");

			IList<IPersonSkill> personSkillCollection = new List<IPersonSkill>();
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillA", 1));
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillB", 1));
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillC", 1));
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillF", 1));

            _target = new PersonPeriodModel(_from3, person, personSkillCollection, null, null, null);

		    IPerson person1 = GetPerson("Test 2");

			IList<IPersonSkill> personSkillCollection1 = new List<IPersonSkill>();
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillC", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillD", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillE", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillF", 1));

            _personPeriodModel = new PersonPeriodModel(_from3, person1, personSkillCollection1, null, null, null);
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

			_personPeriod1 = null;
			_personPeriod2 = null;
			_personPeriod3 = null;

			personPeriodPersonSkillComparer = null;
		}

		#endregion

	    private IPerson GetPerson(string personName)
        {
            IPerson person = PersonFactory.CreatePerson(personName);

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(DateTime.MinValue.AddYears(2)), new Team());
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(DateTime.Now.AddYears(50)), new Team());
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, new Team());

            IList<IPersonPeriod> periodList = new List<IPersonPeriod>();
            periodList.Add(_personPeriod1);
            periodList.Add(_personPeriod2);
            periodList.Add(_personPeriod3);

            person = PersonFactory.GetPerson(person, periodList);

            return person;
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
			// No need to set null skills since we are not setting the skills in the set up process.

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

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
			// No need to set the skill to null since we are not creating any 
			// skill in the set up process.

			// Sets the current person cotnract
			_personPeriodModel.PersonSkills = "_skillD";

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

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
			// Sets the current person cotnract
			_target.PersonSkills = "_skillA";

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

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
			// Sets the current person cotnract
			_target.PersonSkills = "_skillA";
			_personPeriodModel.PersonSkills = "_skillD";

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

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
			// Sets the current person cotnract
			_target.PersonSkills = "_skillF";
			_target.PersonSkills = "_skillA";

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

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
			// Sets the current person cotnract
			_target.PersonSkills = "_skillF";
			_personPeriodModel.PersonSkills = "_skillF";

			// Calls the compares method
			personPeriodPersonSkillComparer = new PersonPeriodPersonSkillComparer();
			result = personPeriodPersonSkillComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
