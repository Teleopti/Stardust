using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for TeamBelongsToSiteSpecification
    /// </summary>
    [TestFixture]
    public class TeamBelongsToSiteSpecificationTest
    {
        private BusinessUnit _bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();

        [Test]
        public void VerifyIsSpecifiedWithMultipleSite()
        {
            ITeam teamOk = _bu.SiteCollection[1].TeamCollection[0];
            ITeam teamNotOk = TeamFactory.CreateSimpleTeam("NotOkTeam");

            TeamBelongsToSiteSpecification spec = new TeamBelongsToSiteSpecification(_bu.SiteCollection);
            Assert.IsTrue(spec.IsSatisfiedBy(teamOk));
            Assert.IsFalse(spec.IsSatisfiedBy(teamNotOk));
        }

        [Test]
        public void VerifyIsSpecifiedWithSingleSite()
        {
            ITeam teamOk = _bu.SiteCollection[1].TeamCollection[0];
            ITeam teamNotOk = TeamFactory.CreateSimpleTeam("NotOkTeam");

            TeamBelongsToSiteSpecification spec = new TeamBelongsToSiteSpecification(_bu.SiteCollection[1]);
            Assert.IsTrue(spec.IsSatisfiedBy(teamOk));
            Assert.IsFalse(spec.IsSatisfiedBy(teamNotOk));
        }
    }
}