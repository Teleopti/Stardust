using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for PersonBelongsToBusinessUnitSpecification
    /// </summary>
    [TestFixture]
    public class PersonBelongsToBusinessUnitSpecificationTest
    {
        private DateOnlyPeriod _periodInQuestion = new DateOnlyPeriod(2007, 1, 1, 2008, 1, 1);

        private BusinessUnit _bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();

        /// <summary>
        /// Verifies the agent belongs to site specification finds correct team period and site.
        /// </summary>
        [Test]
        public void VerifySpecificationHandlesBusinessUnit()
        {
            ITeam okTeam = _bu.SiteCollection[1].TeamCollection[0];
            IPerson okAgent = PersonFactory.CreatePerson();
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 01, 31), okTeam);
            okAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToBusinessUnitSpecification spec =
                new PersonBelongsToBusinessUnitSpecification(_periodInQuestion, _bu);
            Assert.IsTrue(spec.IsSatisfiedBy(okAgent));
            spec = new PersonBelongsToBusinessUnitSpecification(_periodInQuestion, new BusinessUnit("new BU"));
            Assert.IsFalse(spec.IsSatisfiedBy(okAgent));
        }

        [Test]
        public void VerifySpecificationHandlesPeriods()
        {
            ITeam okTeam = _bu.SiteCollection[1].TeamCollection[0];
            IPerson notMemberInPeriodAgent = PersonFactory.CreatePerson();
            var newPeriodInQuestion = new DateOnlyPeriod(_periodInQuestion.EndDate.AddDays(1) , _periodInQuestion.EndDate.AddDays(366));
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(newPeriodInQuestion.StartDate, okTeam);
            notMemberInPeriodAgent.AddPersonPeriod(personPeriod);

            PersonBelongsToBusinessUnitSpecification spec =
                new PersonBelongsToBusinessUnitSpecification(_periodInQuestion, _bu);
            Assert.IsFalse(spec.IsSatisfiedBy(notMemberInPeriodAgent));
        }
    }
}