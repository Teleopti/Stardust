﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
	[LegacyTest]
	public class AuthorizeMyTeamTest
    {
        private IAuthorizeAvailableData target;
        private OrganisationMembership queryingPersonMembership;
        private ISite site;
        private ITeam team;
        private IPerson queryingPerson;

        [SetUp]
        public void Setup()
        {
            queryingPersonMembership = new OrganisationMembership();
            queryingPerson = PersonFactory.CreatePerson();
            target = new AuthorizeMyTeam();
        }

        [Test]
        public void ShouldNotAuthorizePersonWithoutPeriod()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            var targetPerson = PersonFactory.CreatePerson();
            target.Check(queryingPersonMembership, DateOnly.Today, targetPerson).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeTeamWithoutPeriod()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ITeam)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeSite()
        {
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, (ISite)null).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnit()
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
        public void ShouldNotAuthorizeSiteWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);
            
            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.False();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnitWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit).Should().Be.False();
        }
    }
}