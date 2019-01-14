using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class AuthorizeMyOwnTest
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
            queryingPerson.SetId(Guid.NewGuid());
            target = new AuthorizeMyOwn();
        }

        [Test]
        public void ShouldNotAuthorizePersonWithoutPeriod()
        {
            var targetPerson = PersonFactory.CreatePerson();
            targetPerson.SetId(Guid.NewGuid());

            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, targetPerson).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeTeam()
        {
            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ITeam)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeSite()
        {
            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ISite)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnit()
        {
            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (IBusinessUnit)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeOtherPersons()
        {
            SetStructure();
            var targetPerson = PersonFactory.CreatePerson();
            targetPerson.SetId(Guid.NewGuid());

            AddTeamAndSiteToPerson(queryingPerson);
            AddTeamAndSiteToPerson(targetPerson);
            
            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, targetPerson).Should().Be.False();
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
        public void ShouldNotAuthorizeTeamWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, team).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeSiteWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnitWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.Initialize(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit).Should().Be.False();
        }
    }
}