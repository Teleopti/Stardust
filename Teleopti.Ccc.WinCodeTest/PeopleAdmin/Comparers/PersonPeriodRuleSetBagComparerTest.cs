using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PersonPeriodRuleSetBagComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 25-07-2008
	/// </remarks>
	[TestFixture]
	public class PersonPeriodRuleSetBagComparerTest
	{
	    private IPerson _person, _person1;
		private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private readonly List<IPersonSkill> _personSkillCollection = new List<IPersonSkill>();
		private readonly List<IPersonSkill> _personSkillCollection1 = new List<IPersonSkill>();
		private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3, _personPeriod4;
		private PersonPeriodRuleSetBagComparer _personPeriodRuleSetBagComparer;
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
		    #region Set _target

		    _person = PersonFactory.CreatePerson("Test 1");

		    RuleSetBag ruleSetBag1 = new RuleSetBag();
		    ruleSetBag1.Description = new Description("Test A");

		    RuleSetBag ruleSetBag2 = new RuleSetBag();
		    ruleSetBag2.Description = new Description("Test C");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, new Team(), ruleSetBag1);
		    _personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, new Team(), ruleSetBag2);

		    _person.AddPersonPeriod(_personPeriod1);
		    _person.AddPersonPeriod(_personPeriod2);

		    _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill1", 1));
		    _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill2", 1));
		    _personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill3", 1));

		    #endregion

		    #region Set _personPeriodModel

            _person1 = PersonFactory.CreatePerson("Test 2");

            RuleSetBag ruleSetBag3 = new RuleSetBag();
            ruleSetBag3.Description = new Description("Test A");

            RuleSetBag ruleSetBag4 = new RuleSetBag();
            ruleSetBag4.Description = new Description("Test B");

            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, new Team(), ruleSetBag3);
            _personPeriod4 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, new Team(), ruleSetBag4);

		    _person1.AddPersonPeriod(_personPeriod3);
		    _person1.AddPersonPeriod(_personPeriod4);

		    _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill4", 1));
		    _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill5", 1));
		    _personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill6", 1));

		    #endregion
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

			_personPeriodRuleSetBagComparer = null;
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
            _target = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

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
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime3.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

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
            _target = new PersonPeriodModel(new DateOnly(_universalTime1.Date), _person, _personSkillCollection, null, null, null);
            _personPeriodModel = new PersonPeriodModel(new DateOnly(_universalTime2.Date), _person1, _personSkillCollection1, null, null, null);

			// Calls the compares method
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

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
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

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
			_personPeriodRuleSetBagComparer = new PersonPeriodRuleSetBagComparer();
			result = _personPeriodRuleSetBagComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
