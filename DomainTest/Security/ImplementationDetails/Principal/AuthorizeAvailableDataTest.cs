using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
    public class AuthorizeAvailableDataTest
    {
        private IAvailableData availableData;
        private IAuthorizeAvailableData target;
        private IPerson queryingPerson;
        private OrganisationMembership queryingPersonMembership;

        [SetUp]
        public void Setup()
        {
            availableData = new AvailableData();
            queryingPersonMembership = new OrganisationMembership();
            queryingPerson = PersonFactory.CreatePerson();
        }

        [Test]
        public void ShouldNotAuthorizePersonNotInAvailableData()
        {
            IPerson personToCheck = PersonFactory.CreatePerson();

            queryingPersonMembership.Initialize(queryingPerson);

            target = AuthorizeExternalAvailableData.Create(availableData);
            target.Check(queryingPersonMembership, DateOnly.Today, personToCheck).Should().Be.False();
        }

        [Test]
        public void ShouldAuthorizePersonWithTeamInAvailableData()
        {
            IPerson personToCheck = PersonFactory.CreatePerson();
            ITeam team = TeamFactory.CreateSimpleTeam();
            personToCheck.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today,team));

            availableData.AddAvailableTeam(team);

            queryingPersonMembership.Initialize(queryingPerson);

            target = AuthorizeExternalAvailableData.Create(availableData);
            target.Check(queryingPersonMembership, DateOnly.Today, personToCheck).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizePersonWithSiteInAvailableData()
        {
            IPerson personToCheck = PersonFactory.CreatePerson();
            ITeam team = TeamFactory.CreateSimpleTeam();
            team.Site = SiteFactory.CreateSimpleSite();
            personToCheck.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));

            availableData.AddAvailableSite(team.Site);

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, personToCheck).Should().Be.True();
        }

        [Test]
        public void ShouldNotAuthorizePersonWithBusinessUnitNotInAvailableData()
        {
            IPerson personToCheck = PersonFactory.CreatePerson();
            ITeam team = TeamFactory.CreateSimpleTeam();
            team.Site = SiteFactory.CreateSimpleSite();
            personToCheck.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, personToCheck).Should().Be.False();
        }

        [Test]
        public void ShouldAuthorizeTeamWithTeamInAvailableData()
        {
            ITeam team = TeamFactory.CreateSimpleTeam();
            
            availableData.AddAvailableTeam(team);

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, team).Should().Be.True();
        }

        [Test]
        public void ShouldAuthorizeTeamWithSiteInAvailableData()
        {
            ITeam team = TeamFactory.CreateSimpleTeam();
            team.Site = SiteFactory.CreateSimpleSite();
            
            availableData.AddAvailableSite(team.Site);

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, team).Should().Be.True();
        }

        [Test]
        public void ShouldNotAuthorizeTeamWithBusinessUnitNotInAvailableData()
        {
            ITeam team = TeamFactory.CreateSimpleTeam();
            team.Site = SiteFactory.CreateSimpleSite();

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, team).Should().Be.False();
        }

        [Test]
        public void ShouldAuthorizeSiteWithSiteInAvailableData()
        {
            var site = SiteFactory.CreateSimpleSite();

            availableData.AddAvailableSite(site);

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.True();
        }

        [Test]
        public void ShouldNotAuthorizeSiteWithBusinessUnitNotInAvailableData()
        {
            var site = SiteFactory.CreateSimpleSite();

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, site).Should().Be.False();
        }

        [Test]
        public void ShouldAuthorizeBusinessUnitWithBusinessUnitInAvailableData()
        {
            availableData.AddAvailableBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);

            queryingPersonMembership.Initialize(queryingPerson);

			target = AuthorizeExternalAvailableData.Create(availableData);
			target.Check(queryingPersonMembership, DateOnly.Today, BusinessUnitFactory.BusinessUnitUsedInTest).Should().Be.True();
        }

    }
}
