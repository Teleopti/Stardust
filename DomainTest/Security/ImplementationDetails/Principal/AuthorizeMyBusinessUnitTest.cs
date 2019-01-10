using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class AuthorizeMyBusinessUnitTest
    {
        private IAuthorizeAvailableData target;
        private IPerson queryingPerson;
        private ISite site;
        private ITeam team;
        private OrganisationMembership queryingPersonMembership;

        [SetUp]
        public void Setup()
        {
            queryingPersonMembership = new OrganisationMembership();
            queryingPerson = PersonFactory.CreatePerson();
            target = new AuthorizeMyBusinessUnit();
        }

        [Test]
        public void ShouldNotAuthorizePersonWithoutPeriod()
        {
            var targetPerson = PersonFactory.CreatePerson();
            
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, targetPerson).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeTeamWithoutPeriod()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ITeam)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeSiteWithoutPeriod()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ISite)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnitWithoutPeriod()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (IBusinessUnit)null).Should().Be.False();
        }

       
        [Test]
        public void ShouldAuthorizePersonWithPeriod()
        {
            SetStructure();
            var targetPerson = PersonFactory.CreatePerson();
            AddTeamAndSiteToPerson(queryingPerson);
            AddTeamAndSiteToPerson(targetPerson);
            
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, targetPerson).Should().Be.True();
        }

        private void AddTeamAndSiteToPerson(IPerson person)
        {
            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));
        }

        private void SetStructure()
        {
            site = SiteFactory.CreateSimpleSite();
            team = TeamFactory.CreateSimpleTeam();

            site.AddTeam(team);
        }

        [Test]
        public void ShouldAuthorizeTeamWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);
            
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, team).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeSiteWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeBusinessUnitWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit).Should().Be.True();
        }
    }
}