﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class AuthorizeMySiteTest
    {
        private IAuthorizeAvailableData target;
        private IPerson queryingPerson;
        private ISite site;
        private ITeam team;
        private IOrganisationMembership queryingPersonMembership;

        [SetUp]
        public void Setup()
        {
            queryingPersonMembership = new OrganisationMembership();
            queryingPerson = PersonFactory.CreatePerson();
            target = new AuthorizeMySite();
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
        public void ShouldAuthorizeSiteWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.True();
        }

        [Test]
        public void ShouldNotAuthorizeBusinessUnitWithPeriod()
        {
            SetStructure();
            AddTeamAndSiteToPerson(queryingPerson);

            queryingPersonMembership.AddFromPerson(queryingPerson);
            target.Check(queryingPersonMembership, DateOnly.Today, ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit).Should().Be.False();
        }
    }
}