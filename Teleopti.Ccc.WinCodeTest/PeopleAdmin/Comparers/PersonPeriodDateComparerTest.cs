using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PersonPeriodDateComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 23-07-2008
	/// </remarks>
	[TestFixture]
	public class PersonPeriodDateComparerTest
	{
	    private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
        private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3, _personPeriod4, _personPeriod5; //, _personPeriod6;
		private PersonPeriodDateComparer personPeriodDateComparer;
		private int result;
	    private IPerson person, person1;
        private IList<IPersonSkill> personSkillCollection, personSkillCollection1;

        private DateOnly _universalTime1 = new DateOnly(2058, 10, 09);
        private DateOnly _universalTime2 = new DateOnly(2010, 10, 09);
        private DateOnly _universalTime3 = new DateOnly(2008, 10, 09);

        private DateOnly dateTime3, dateTime4;

        [SetUp]
        public void Setup()
		{
		    dateTime3 = new DateOnly(2008, 10, 09);
            dateTime4 = new DateOnly(2008, 10, 10);

            person = PersonFactory.CreatePerson("Test 1");

		    _personPeriod1 =
		        PersonPeriodFactory.CreatePersonPeriodWithSkills(_universalTime2, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill1"));
		    _personPeriod2 =
		        PersonPeriodFactory.CreatePersonPeriodWithSkills(_universalTime1, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill2"));
		    _personPeriod3 =
		        PersonPeriodFactory.CreatePersonPeriodWithSkills(_universalTime3, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill3"));

		    personSkillCollection = new List<IPersonSkill>
		                                {
		                                    PersonSkillFactory.CreatePersonSkill("_skill1", 1),
		                                    PersonSkillFactory.CreatePersonSkill("_skill2", 1),
		                                    PersonSkillFactory.CreatePersonSkill("_skill3", 1)
		                                };

            person1 = PersonFactory.CreatePerson("Test 2");

		    _personPeriod4 =
		        PersonPeriodFactory.CreatePersonPeriodWithSkills(_universalTime2, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill4"));
		    _personPeriod5 =
		        PersonPeriodFactory.CreatePersonPeriodWithSkills(_universalTime3, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill5"));

		    personSkillCollection1 = new List<IPersonSkill>
		                                 {
		                                     PersonSkillFactory.CreatePersonSkill("_skill4", 1),
		                                     PersonSkillFactory.CreatePersonSkill("_skill5", 1),
		                                     PersonSkillFactory.CreatePersonSkill("_skill6", 1)
		                                 };
		}

		[TearDown]
		public void TestDispose()
		{
			// Sets teh objects to null
			_target = null;
			_personPeriodModel = null;

			_personPeriod1 = null;
			_personPeriod2 = null;
			_personPeriod3 = null;

			personPeriodDateComparer = null;
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
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

            _target.Contract = null;
            _personPeriodModel.Contract = null;

            // Calls the compares method
            personPeriodDateComparer = new PersonPeriodDateComparer();
            result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);

            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

            // Calls the compares method
            personPeriodDateComparer = new PersonPeriodDateComparer();
            result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

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
            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);

            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

            // Calls the compares method
            personPeriodDateComparer = new PersonPeriodDateComparer();
            result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

		/// <summary>
		/// Verifies the compare method ascending values for all parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithAscending()
		{
            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

			// Sets the period date of the person period
            _target.PeriodDate = dateTime3;
            _personPeriodModel.PeriodDate = dateTime4;

			// Calls the compares method
			personPeriodDateComparer = new PersonPeriodDateComparer();
			result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method descending values for all parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithDescending()
		{
            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

			// Sets the period date of the person period
			_target.PeriodDate = dateTime4;
		    _personPeriodModel.PeriodDate = dateTime3;

			// Calls the compares method
			personPeriodDateComparer = new PersonPeriodDateComparer();
			result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with same period date of both parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person, personSkillCollection, null, null, null);

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), person1, personSkillCollection1, null, null, null);

			// Sets the period date of the person period
            _target.PeriodDate = dateTime3;
            _personPeriodModel.PeriodDate = dateTime3;

			// Calls the compares method
			personPeriodDateComparer = new PersonPeriodDateComparer();
			result = personPeriodDateComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
