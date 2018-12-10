#region Imports

using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Test class for the PersonPeriodExternalLogOnNameComparer class of the wincode.
    /// </summary>
    /// <remarks>
    /// Created By: 
    /// Created Date: 24-07-2008
    /// </remarks>
    [TestFixture]
    public class PersonPeriodExternalLogOnNameComparerTest
    {
        private PersonPeriodModel _target;
        private PersonPeriodModel _personPeriodModel;
        private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3, _personPeriod4, _personPeriod5, _personPeriod6;
        private PersonPeriodExternalLogOnNameComparer personPeriodExternalLogOnNameComparer;
        private int result;

        private DateOnly _universalTime1 = new DateOnly(2058, 10, 09);
        private DateOnly _universalTime2 = new DateOnly(2010, 10, 09);
        private DateOnly _universalTime3 = new DateOnly(2008, 10, 09);

        private static IEnumerable<IExternalLogOn> GetExternalLogOnData()
        {
            IList<IExternalLogOn> externalLogOnCollection = new List<IExternalLogOn>();

            for (int index = 0; index < 4; index++)
            {
                externalLogOnCollection.Add(ExternalLogOnFactory.CreateExternalLogOn());
            }

            return externalLogOnCollection;
        }

        [SetUp]
        public void Setup()
        {
            IPerson person = PersonFactory.CreatePerson("Test 1");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, new Team(), new RuleSetBag());
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, new Team(), new RuleSetBag());
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(_universalTime3, new Team(), new RuleSetBag());

            person.AddPersonPeriod(_personPeriod1);
            person.AddPersonPeriod(_personPeriod2);
            person.AddPersonPeriod(_personPeriod3);

            IList<IPersonSkill> personSkillCollection = new List<IPersonSkill>();
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillA", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillB", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillC", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skillF", 1));

            _target = new PersonPeriodModel(_universalTime3, person, personSkillCollection, GetExternalLogOnData(), null, null);

            IPerson person1 = PersonFactory.CreatePerson("Test 2");

            _personPeriod4 = PersonPeriodFactory.CreatePersonPeriod(_universalTime2, new Team(), new RuleSetBag());
            _personPeriod5 = PersonPeriodFactory.CreatePersonPeriod(_universalTime1, new Team(), new RuleSetBag());
            _personPeriod6 = PersonPeriodFactory.CreatePersonPeriod(_universalTime3, new Team(), new RuleSetBag());

            person1.AddPersonPeriod(_personPeriod4);
            person1.AddPersonPeriod(_personPeriod5);
            person1.AddPersonPeriod(_personPeriod6);

            IList<IPersonSkill> personSkillCollection1 = new List<IPersonSkill>();

            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillC", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillD", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillE", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skillF", 1));

            _personPeriodModel = new PersonPeriodModel(_universalTime3, person1, personSkillCollection1, GetExternalLogOnData(), null, null);
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

            personPeriodExternalLogOnNameComparer = null;
        }

        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            // No need to set null skills since we are not setting the skills in the set up process.

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            _personPeriodModel.ExternalLogOnNames = "Login name (DS)";

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);
            
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            // Sets the current person cotnract
            _target.ExternalLogOnNames = "Login name (DS)";

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        [Test]
        public void VerifyCompareMethodAscending()
        {
            // Sets the current person cotnract
            _target.ExternalLogOnNames = "Login name (3)";
            _personPeriodModel.ExternalLogOnNames = "Login name (3)";

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifyCompareMethodDescending()
        {
            // Sets the current person cotnract
            _target.ExternalLogOnNames = "Login name (3)";
            _personPeriodModel.ExternalLogOnNames = "Login name (3)";

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
            // Sets the current person cotnract
            _target.ExternalLogOnNames = "Login name (3)";
            _personPeriodModel.ExternalLogOnNames = "Login name (3)";

            // Calls the compares method
            personPeriodExternalLogOnNameComparer = new PersonPeriodExternalLogOnNameComparer();
            result = personPeriodExternalLogOnNameComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }
    }
}
