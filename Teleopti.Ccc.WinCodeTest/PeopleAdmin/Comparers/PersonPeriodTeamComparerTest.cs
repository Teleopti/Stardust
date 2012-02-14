using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Test class for the PersonPeriodTeamComparer class of the wincode.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 29-07-2008
    /// </remarks>
    [TestFixture]
    public class PersonPeriodTeamComparerTest
    {
        private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
        private PersonPeriodTeamComparer personPeriodTeamComparer;
		private int result;
        private SchedulePeriodComparerTestHelper helper = new SchedulePeriodComparerTestHelper();

        /// <summary>
        /// Tests the init.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            helper.SetFirstTarget();
            helper.SetSecondtarget();

            IList<IPersonSkill> personSkillCollection = new List<IPersonSkill>();

            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill1", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill2", 1));
            personSkillCollection.Add(PersonSkillFactory.CreatePersonSkill("_skill3", 1));

            _target = new PersonPeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, personSkillCollection, null, null, null);

            IList<IPersonSkill> personSkillCollection1 = new List<IPersonSkill>();

            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill4", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill5", 1));
            personSkillCollection1.Add(PersonSkillFactory.CreatePersonSkill("_skill6", 1));

            _personPeriodModel =
                new PersonPeriodModel(new DateOnly(helper.universalTime3.Date), helper.person1, personSkillCollection1, null, null, null);
        }

        /// <summary>
        /// Tests the dispose.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            // Sets teh objects to null
            _target = null;
            _personPeriodModel = null;

            personPeriodTeamComparer = null;
        }

        /// <summary>
        /// Verifies the compare method with null values for all parameters.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            _target.Team = null;
            _personPeriodModel.Team = null;

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the first parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            _target.Team = null;
            _personPeriodModel.Team = new Team();
            _personPeriodModel.Team.Description = new Description("Test A");

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the second parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            _target.Team = new Team();
            _target.Team.Description = new Description("Test A");
            _personPeriodModel.Team = null;

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with a for the first parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodAscending()
        {
            _target.Team = new Team();
            _target.Team.Description = new Description("Test A");
            _personPeriodModel.Team = new Team();
            _personPeriodModel.Team.Description = new Description("Test B");

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with a for teh second parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodDescending()
        {
            _target.Team = new Team();
            _target.Team.Description = new Description("Test B");
            _personPeriodModel.Team = new Team();
            _personPeriodModel.Team.Description = new Description("Test A");

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with same role for both parameters.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
            _target.Team = new Team();
            _target.Team.Description = new Description("Test A");
            _personPeriodModel.Team = new Team();
            _personPeriodModel.Team.Description = new Description("Test A");

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }
    }
}
