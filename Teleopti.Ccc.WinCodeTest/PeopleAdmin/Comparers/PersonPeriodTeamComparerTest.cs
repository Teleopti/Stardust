using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    [TestFixture]
    public class PersonPeriodTeamComparerTest
    {
        private PersonPeriodModel _target;
		private PersonPeriodModel _personPeriodModel;
        private PersonPeriodTeamComparer personPeriodTeamComparer;
		private int result;
        private readonly SchedulePeriodComparerTestHelper helper = new SchedulePeriodComparerTestHelper();

        [SetUp]
        public void Setup()
        {
            helper.SetFirstTarget();
            helper.SetSecondtarget();

	        IList<IPersonSkill> personSkillCollection = new List<IPersonSkill>
		        {
			        PersonSkillFactory.CreatePersonSkill("_skill1", 1),
			        PersonSkillFactory.CreatePersonSkill("_skill2", 1),
			        PersonSkillFactory.CreatePersonSkill("_skill3", 1)
		        };

	        _target = new PersonPeriodModel(helper.universalTime3, helper.person, personSkillCollection, null,
	                                        new List<SiteTeamModel>
		                                        {
			                                        new SiteTeamModel {ContainedEntity = helper._personPeriod3.Team}
		                                        }, null);

	        personSkillCollection = new List<IPersonSkill>
		        {
			        PersonSkillFactory.CreatePersonSkill("_skill4", 1),
			        PersonSkillFactory.CreatePersonSkill("_skill5", 1),
			        PersonSkillFactory.CreatePersonSkill("_skill6", 1)
		        };

	        _personPeriodModel = new PersonPeriodModel(helper.universalTime3, helper.person1, personSkillCollection, null,
	                                                   new List<SiteTeamModel>
		                                                   {
			                                                   new SiteTeamModel
				                                                   {
					                                                   ContainedEntity = helper._personPeriod6.Team
				                                                   }
		                                                   }, null);
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
            _personPeriodModel = null;

            personPeriodTeamComparer = null;
        }

        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            _target.SiteTeam = null;
            _personPeriodModel.SiteTeam = null;

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            _target.SiteTeam = null;
            _personPeriodModel.SiteTeam.Team.SetDescription(new Description("Test A"));

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            _target.SiteTeam.Team.SetDescription(new Description("Test A"));
            _personPeriodModel.SiteTeam = null;

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        [Test]
        public void VerifyCompareMethodAscending()
        {
			_target.SiteTeam.Team.SetDescription(new Description("Test A"));
			_personPeriodModel.SiteTeam.Team.SetDescription(new Description("Test B"));

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void VerifyCompareMethodDescending()
        {
			_target.SiteTeam.Team.SetDescription(new Description("Test B"));
			_personPeriodModel.SiteTeam.Team.SetDescription(new Description("Test A"));

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
			_target.SiteTeam.Team.SetDescription(new Description("Test A"));
			_personPeriodModel.SiteTeam.Team.SetDescription(new Description("Test A"));

            // Calls the compares method
            personPeriodTeamComparer = new PersonPeriodTeamComparer();
            result = personPeriodTeamComparer.Compare(_target, _personPeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }
    }
}
