using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for PersonBelongsToSiteSpecification
    /// </summary>
    [TestFixture]
    public class PersonBelongsToSiteSpecificationTest
    {
        private DateOnlyPeriod _periodInQuestion = new DateOnlyPeriod(2007, 1, 1, 2008, 1, 1);

        private BusinessUnit _bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();

        /// <summary>
        /// Verifies the agent belongs to site specification finds correct team period and site.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToSiteSpecificationFindsCorrectTeamPeriodAndSite()
        {
            ITeam okTeam = _bu.SiteCollection[1].TeamCollection[0];
            IPerson okAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 01, 31), okTeam);
            okAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToSiteSpecification spec =
                new PersonBelongsToSiteSpecification(_periodInQuestion, _bu.SiteCollection[1]);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));
            spec = new PersonBelongsToSiteSpecification(_periodInQuestion, _bu.SiteCollection[0]);
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to site specification does handle teams and site.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToSiteSpecificationDoesHandleTeamsAndSites()
        {
            ITeam falseTeam = TeamFactory.CreateSimpleTeam();
            IPerson notMemberOfTeamAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(_periodInQuestion.StartDate, falseTeam);
            notMemberOfTeamAgent.AddPersonPeriod(personPeriod);
          
            PersonBelongsToSiteSpecification spec =
                new PersonBelongsToSiteSpecification(_periodInQuestion, _bu.SiteCollection[1]);
            Assert.IsFalse(spec.IsSatisfiedBy(notMemberOfTeamAgent));
        }

        /// <summary>
        /// Verifies the agent belongs to site specification does handle periods.
        /// </summary>
        [Test]
        public void VerifyAgentBelongsToSiteSpecificationDoesHandlePeriods()
        {
            ITeam okTeam = _bu.SiteCollection[1].TeamCollection[0];
            IPerson notMemberInPeriodAgent = PersonFactory.CreatePerson();
            var newPeriodInQuestion = new DateOnlyPeriod(_periodInQuestion.EndDate.AddDays(1) , _periodInQuestion.EndDate.AddDays(366));
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(newPeriodInQuestion.StartDate, okTeam);
            notMemberInPeriodAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToSiteSpecification spec =
                new PersonBelongsToSiteSpecification(_periodInQuestion, _bu.SiteCollection[1]);
            Assert.IsFalse(spec.IsSatisfiedBy(notMemberInPeriodAgent));
        }
    }
}