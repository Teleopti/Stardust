using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PersonPeriodContractComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 23-07-2008
	/// </remarks>
	[TestFixture]
	public class PersonPeriodContractComparerTest
	{
	    private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
		private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3,
			_personPeriod4, _personPeriod5, _personPeriod6;
		private PersonPeriodContractComparer personPeriodContractComparer;
		private int result;

	    [SetUp]
		public void Setup()
		{
            DateOnly from1 = new DateOnly(2058, 10, 09);
            DateOnly from2 = new DateOnly(2010, 10, 09);
            DateOnly from3 = new DateOnly(2008, 10, 09);

	        IPerson person = PersonFactory.CreatePerson("Test 1");

		    _personPeriod1 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from2,
                                                                 new RuleSetBag(), SkillFactory.CreateSkill("_skill1"));
		    _personPeriod2 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from1,
                                                                 new RuleSetBag(), SkillFactory.CreateSkill("_skill1"));
		    _personPeriod3 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from3, new RuleSetBag(), SkillFactory.CreateSkill("_skill3"));

			person.AddPersonPeriod(_personPeriod1);
			person.AddPersonPeriod(_personPeriod2);
			person.AddPersonPeriod(_personPeriod3);

			IList<IPersonSkill> personSkillCollection = new List<IPersonSkill>();
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill1", 1));
			personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill2", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill3", 1));

            _target = new PersonPeriodModel(from3, person, personSkillCollection, null, null, null);

	        IPerson person1 = PersonFactory.CreatePerson("Test 2");

		    _personPeriod4 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from2
                                                                 , new RuleSetBag(), SkillFactory.CreateSkill("_skill4"));
		    _personPeriod5 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from1, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill5"));
		    _personPeriod6 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(from3, new RuleSetBag(),
		                                                         SkillFactory.CreateSkill("_skill6"));

			person1.AddPersonPeriod(_personPeriod4);
			person1.AddPersonPeriod(_personPeriod5);
			person1.AddPersonPeriod(_personPeriod6);

			IList<IPersonSkill> personSkillCollection1 = new List<IPersonSkill>();
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill4", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill5", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill6", 1));

            _personPeriodModel = new PersonPeriodModel(from3, person1, personSkillCollection1, null, null, null);
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

			personPeriodContractComparer = null;
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
			// Sets the current person cotnract
			_target.PersonContract = null;
			_personPeriodModel.PersonContract = null;

			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

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
			// Setting the person contract to null is enough since 
			// PersonPeriodFactory has created a default contract 
			// when setting up the test.

			// Sets the current person cotnract
			_target.PersonContract =null;

			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

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
			// Setting the person contract to null is enough since 
			// PersonPeriodFactory has created a default contract 
			// when setting up the test.

			// Sets the current person cotnract
			_personPeriodModel.PersonContract = null;

			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

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
			_target.PersonContract = new PersonContract(new Contract("my first contract"),
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
			_personPeriodModel.PersonContract = new PersonContract(new Contract("my second contract"), 
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));

			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

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
			_target.PersonContract = new PersonContract(new Contract("my second contract"),
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
			_personPeriodModel.PersonContract = new PersonContract(new Contract("my first contract"),
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));

			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

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
			// Sets the current person cotnract. contract name: my contract
			_target.PersonContract = new PersonContract(new Contract("my first contract"),
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
			_personPeriodModel.PersonContract = new PersonContract(new Contract("my first contract"),
				new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
			// Calls the compares method
			personPeriodContractComparer = new PersonPeriodContractComparer();
			result = personPeriodContractComparer.Compare(_target, _personPeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
