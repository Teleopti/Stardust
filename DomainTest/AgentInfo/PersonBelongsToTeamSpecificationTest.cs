using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for PersonBelongsToTeamSpecification
    /// </summary>
    [TestFixture]
    public class PersonBelongsToTeamSpecificationTest
    {
        private DateOnlyPeriod _periodInQuestion =
            new DateOnlyPeriod(2007, 1, 1, 2008, 1, 1);

        private DateOnly _dateInQuestion = new DateOnly(2007, 2, 1);

        private ITeam _okTeam = TeamFactory.CreateSimpleTeam("OKTeam");
        private ITeam _notOkTeam = TeamFactory.CreateSimpleTeam("NotOKTeam");

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectDateTimePeriod()
        {
            IPerson okAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 01, 31), _okTeam);
            okAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(_periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase1()
        {
            Team notOkTeam2 = TeamFactory.CreateSimpleTeam();

            DateOnlyPeriod periodInQuestion = new DateOnlyPeriod(2007, 4, 1, 2007, 4, 10);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod notOkPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _notOkTeam);
            okAgent.AddPersonPeriod(notOkPeriod1);

            IPersonPeriod notOkPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), notOkTeam2);
            okAgent.AddPersonPeriod(notOkPeriod2);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _notOkTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, notOkTeam2);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));

        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase2()
        {

            var periodInQuestion = new DateOnlyPeriod(2007, 4, 1, 2007, 5, 10);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod okPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _okTeam);
            okAgent.AddPersonPeriod(okPeriod);

            IPersonPeriod notOkPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), _notOkTeam);
            okAgent.AddPersonPeriod(notOkPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _notOkTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase3()
        {
            var periodInQuestion = new DateOnlyPeriod(2007, 5, 10, 2007, 5, 30);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod okPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _okTeam);
            okAgent.AddPersonPeriod(okPeriod);

            IPersonPeriod notOkPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), _notOkTeam);
            okAgent.AddPersonPeriod(notOkPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, _notOkTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase4()
        {
            Team okTeam2 = TeamFactory.CreateSimpleTeam();

            var periodInQuestion = new DateOnlyPeriod(2007, 5, 10, 2007, 7, 10);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod okPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _okTeam);
            okAgent.AddPersonPeriod(okPeriod1);

            IPersonPeriod okPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), okTeam2);
            okAgent.AddPersonPeriod(okPeriod2);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, okTeam2);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase5()
        {

            var periodInQuestion = new DateOnlyPeriod(2007, 6, 2,2007, 6, 10);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod notOkPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _notOkTeam);
            okAgent.AddPersonPeriod(notOkPeriod);

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), _okTeam);
            okAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _notOkTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to team specification finds correct team with correct period.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationFindsCorrectTeamCase6()
        {
            ITeam okTeam2 = TeamFactory.CreateSimpleTeam();

            var periodInQuestion = new DateOnlyPeriod(2007, 4, 1, 2007, 6, 10);

            IPerson okAgent = PersonFactory.CreatePerson();

            IPersonPeriod okPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 5, 1), _okTeam);
            okAgent.AddPersonPeriod(okPeriod);

            IPersonPeriod okPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 6, 1), okTeam2);
            okAgent.AddPersonPeriod(okPeriod2);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(periodInQuestion, okTeam2);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to team specification does handle teams.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationDoesHandleTeams()
        {
            ITeam falseTeam = TeamFactory.CreateSimpleTeam();
            IPerson notMemberOfTeamAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(_periodInQuestion.StartDate, _okTeam);
            notMemberOfTeamAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(_periodInQuestion, falseTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(notMemberOfTeamAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to team specification does handle periods.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToTeamSpecificationDoesHandlePeriods()
        {
            IPerson notMemberInPeriodAgent = PersonFactory.CreatePerson();
            var period = new DateOnlyPeriod(_periodInQuestion.EndDate.AddDays(1), _periodInQuestion.EndDate.AddDays(366));
            IPersonPeriod newPersonPeriod = PersonPeriodFactory.CreatePersonPeriod(period.StartDate, _okTeam);
            notMemberInPeriodAgent.AddPersonPeriod(newPersonPeriod);

            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(_periodInQuestion, _okTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(notMemberInPeriodAgent));
        }

        [Test]
        public void VerifyPeriodAfterLastTeamChangeIsConsideredToBeOnLastTeam()
        {
            IPerson okAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 01, 31), _okTeam);
            okAgent.AddPersonPeriod(personPeriod);
          
            PersonBelongsToTeamSpecification spec = new PersonBelongsToTeamSpecification(_periodInQuestion, _okTeam);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));

            spec = new PersonBelongsToTeamSpecification(_periodInQuestion, _notOkTeam);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));
        }

        [Test]
        public void VerifyListOfTeamsWork()
        {
            ITeam okTeam2 = TeamFactory.CreateSimpleTeam();
            ITeam falseTeam = TeamFactory.CreateSimpleTeam();

            IPerson okAgent1 = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 12, 31), _okTeam);
            okAgent1.AddPersonPeriod(personPeriod1);

            IPerson okAgent2 = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 12, 31), okTeam2);
            okAgent2.AddPersonPeriod(personPeriod2);

            IPerson falseAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 12, 31), falseTeam);
            falseAgent.AddPersonPeriod(personPeriod3);

            IList<ITeam> teams = new List<ITeam> { okTeam2, _okTeam };
            IList<IPerson> filteredTeams = new List<IPerson> { okAgent1, okAgent2, falseAgent }.
                    FindAll(new PersonBelongsToTeamSpecification(_periodInQuestion, teams).IsSatisfiedBy);

            Assert.AreEqual(filteredTeams.Count, 2);
            Assert.IsTrue(filteredTeams.Contains(okAgent1));
            Assert.IsTrue(filteredTeams.Contains(okAgent2));

        }
    }
}