

using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    /// <summary>
    /// Tests for ChildList
    /// </summary>
    [TestFixture]
    public class ChildListTest
    {
        private ChildList _target;
        private PersonPeriod personPeriod;
        ChildList childList1;

        [SetUp]
        public void Setup()
        {
            personPeriod = PersonPeriodFactory.CreatePersonPeriod
                (DateTime.Now,
                 PersonContractFactory.CreatePersonContract("testContract",
                                                            "TestSchedule",
                                                            "TestPartTimePercentage"),
                 new Team());
            _target = new ChildList
                (personPeriod);
            childList1 = new ChildList(null);
        }


        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target.PeriodDate);
            Assert.IsNotNull(_target.Skill);
            Assert.IsNotNull(_target.CurrentPeriod);
            Assert.IsNotNull(_target.CurrentTeam);
            Assert.IsNotNull(_target.CurrentPersonContract);
            Assert.IsNotNull(_target.CurrentContractSchedule);
            Assert.IsNotNull(_target.CurrentPartTimePercentage);
        }

        /// <summary>
        /// Verifies the skill property.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifySkillProperty()
        {
            Assert.AreEqual(string.Empty, _target.Skill);

            PersonPeriod personPeriod1 =
                PersonPeriodFactory.CreatePersonPeriodWithSkills(DateTime.MinValue, new Team());
            PersonSkill personSkill = new PersonSkill(SkillFactory.CreateSkill("test"), new Percent(2));
            personPeriod1.AddPersonSkill(personSkill);

            ChildList childList = new ChildList(personPeriod1);

            Assert.AreEqual("test skill;test;", childList.Skill);

           Assert.AreEqual(string.Empty, childList1.Skill);

        }

        /// <summary>
        /// Verifies the current team.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyCurrentTeam()
        {
            Assert.IsNull(childList1.CurrentTeam);
        }

        /// <summary>
        /// Verifies the current contract schedule.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentContractSchedule()
        {
            Assert.IsNull(childList1.CurrentContractSchedule);
        }

        /// <summary>
        /// Verifies the current part time percentage.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentPartTimePercentage()
        {
            Assert.IsNull(childList1.CurrentPartTimePercentage);
        }



        /// <summary>
        /// Verifies the current person contract.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyCurrentPersonContract()
        {
            Assert.IsNull(childList1.CurrentPersonContract);
        }

        /// <summary>
        /// Verifies the period date.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-14
        /// </remarks>
        [Test]
        public void VerifyPeriodDate()
        {
            // Check period date is null
            Assert.IsNull(childList1.PeriodDate);
        }

        /// <summary>
        /// Verifies the period date can set.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyPeriodDateCanSet()
        {
            _target.PeriodDate = DateTime.MinValue;
            Assert.AreEqual(DateTime.MinValue, _target.PeriodDate);
        }

        /// <summary>
        /// Verifies the current team can set.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyCurrentTeamCanSet()
        {
            Team team = new Team();
            Description description = new Description("Team");
            team.Description = description;
            _target.CurrentTeam = team;
            Assert.AreEqual(description, _target.CurrentTeam.Description);

        }


        /// <summary>
        /// Verifies the current contract schedule can set.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentContractScheduleCanSet()
        {
            ContractSchedule contractSchedule = new ContractSchedule("ContractSchedule");
            _target.CurrentContractSchedule = contractSchedule;
            Assert.AreEqual("ContractSchedule", _target.CurrentContractSchedule.Description.Name);
        }

        /// <summary>
        /// Verifies the current part time percentage can set.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentPartTimePercentageCanSet()
        {
            PartTimePercentage partTimePercentage = new PartTimePercentage("PartTimePercentage");
            _target.CurrentPartTimePercentage = partTimePercentage;
            Assert.AreEqual("PartTimePercentage", _target.CurrentPartTimePercentage.Description.Name);
        }


        /// <summary>
        /// Verifies the current person contract can set.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyCurrentPersonContractCanSet()
        {
            PersonContract personContract =
                PersonContractFactory.CreatePersonContract("Contract", "ContractSchedule", "PartTimePercentage");

            _target.CurrentPersonContract = personContract;

            Assert.AreEqual(personContract, _target.CurrentPersonContract);

        }

        /// <summary>
        /// Verifies the current ID.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-16
        /// </remarks>
        [Test]
        public void VerifyCurrentProperties()
        {
            Assert.IsNull(_target.CurrentTeamIdentifier);
            Assert.IsNull(_target.CurrentPersonContractIdentifier);
            Assert.IsNull(_target.CurrentContractScheduleIdentifier);
            Assert.IsNull(_target.CurrentPartTimePercentageIdentifier);
        }

        [Test]
        public void VerifyCurrentTeamIdentifierCanSet()
        {
            _target.CurrentTeamIdentifier = Guid.NewGuid();
            Assert.IsNull(_target.CurrentTeamIdentifier);

        }

        [Test]
        public void VerifyCurrentPersonContractIdentifierCanSet()
        {
            _target.CurrentPersonContractIdentifier = Guid.NewGuid();
            Assert.IsNull(_target.CurrentPersonContractIdentifier);
        }


        /// <summary>
        /// Verifies the current contract schedule identifier can set.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentContractScheduleIdentifierCanSet()
        {
            _target.CurrentContractScheduleIdentifier = Guid.NewGuid();
            Assert.IsNull(_target.CurrentContractScheduleIdentifier);
        }

        /// <summary>
        /// Verifies the current part time percentage identifier can set.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/27/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentPartTimePercentageIdentifierCanSet()
        {
            _target.CurrentPartTimePercentageIdentifier = Guid.NewGuid();
            Assert.IsNull(_target.CurrentPartTimePercentageIdentifier);

        }

    }
}
