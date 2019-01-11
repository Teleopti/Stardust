using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
	public class PrincipalAuthorizationTest
    {
        private IAuthorization authorization;
        private IPerson person;
        private ITeleoptiPrincipal principal;
        private MockRepository mocks;
        private IAuthorizeAvailableData authorizeAvailableData;
        private IApplicationFunction applicationFunction;
        private OrganisationMembership organisationMembership;
        const string Function = "test";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            authorizeAvailableData = mocks.StrictMock<IAuthorizeAvailableData>();
            person = PersonFactory.CreatePerson();
			principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null), person);
            organisationMembership = (OrganisationMembership) principal.Organisation;
			authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));
            applicationFunction = new ApplicationFunction(Function);
            applicationFunction.SetId(Guid.NewGuid());

            PrepareClaims();
        }

        [Test]
        public void ShouldCheckPermissionsForPerson()
        {
            var toCheck = mocks.StrictMock<IPerson>();
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, DateOnly.Today, toCheck)).Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.InitializeFromPerson(person);
                Assert.IsTrue(authorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldCheckPermissionsForFunction()
        {
            organisationMembership.InitializeFromPerson(person);
            Assert.IsTrue(authorization.IsPermitted(Function));
        }

        [Test]
        public void ShouldCheckPermissionsForTeam()
        {
            var toCheck = mocks.StrictMock<ITeam>();
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, DateOnly.Today, toCheck)).Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.InitializeFromPerson(person);
                Assert.IsTrue(authorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldCheckPermissionsForSite()
        {
            var toCheck = mocks.StrictMock<ISite>();
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, DateOnly.Today, toCheck)).Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.InitializeFromPerson(person);
                Assert.IsTrue(authorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldHandlePermittedPeriodsCorrectly()
        {
            var today = DateOnly.Today;
            var otherPerson = PersonFactory.CreatePerson();
            var site = SiteFactory.CreateSiteWithOneTeam();

            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10),site.TeamCollection[0]));
            otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null), person);
			organisationMembership = (OrganisationMembership)principal.Organisation;
			authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));

            PrepareClaims();
            
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-20), otherPerson)).Return(true);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(false);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-10), otherPerson)).Return(true);
            }
            using (mocks.Playback())
            {
                var result = authorization.PermittedPeriods(applicationFunction.FunctionPath,
                                                                     new DateOnlyPeriod(today.AddDays(-20),
                                                                                        today.AddDays(5)), otherPerson);
                result.Count().Should().Be.EqualTo(2);
                result.Contains(new DateOnlyPeriod(today.AddDays(-20),today.AddDays(-16))).Should().Be.True();
                result.Contains(new DateOnlyPeriod(today.AddDays(-10),today.AddDays(5))).Should().Be.True();
            }
        }

        [Test]
        public void ShouldHandlePermittedPeriodsStartingAfterGivenPeriodCorrectly()
        {
            var today = DateOnly.Today;
            var otherPerson = PersonFactory.CreatePerson();
            var site = SiteFactory.CreateSiteWithOneTeam();

            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
            otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null), person);
			organisationMembership = (OrganisationMembership)principal.Organisation;
			authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));

            PrepareClaims();

            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-20), otherPerson)).Return(true);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(false);
            }
            using (mocks.Playback())
            {
                var result = authorization.PermittedPeriods(applicationFunction.FunctionPath,
                                                                     new DateOnlyPeriod(today.AddDays(-20),
                                                                                        today.AddDays(-12)), otherPerson);
                result.Count().Should().Be.EqualTo(1);
                result.Contains(new DateOnlyPeriod(today.AddDays(-20), today.AddDays(-16))).Should().Be.True();
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldHandleTerminalDateProperlyWhenGettingPermittedPeriods()
		{
			var today = DateOnly.Today;
			var otherPerson = PersonFactory.CreatePerson();
			var site = SiteFactory.CreateSiteWithOneTeam();

			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
			otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));
			person.TerminatePerson(today, new PersonAccountUpdaterDummy());

			principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null), person);
			organisationMembership = (OrganisationMembership) principal.Organisation;
			authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));

			PrepareClaims();

			using (mocks.Record())
			{
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(true);
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-10), otherPerson)).Return(true);
			}
			using (mocks.Playback())
			{
				var result = authorization.PermittedPeriods(applicationFunction.FunctionPath,
																	 new DateOnlyPeriod(today.AddDays(-15),
																						today.AddDays(2)), otherPerson);
				result.Count().Should().Be.EqualTo(2);
				result.Contains(new DateOnlyPeriod(today.AddDays(-15), today.AddDays(-11))).Should().Be.True();
				result.Contains(new DateOnlyPeriod(today.AddDays(-10), today)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldHandlePermittedPeriodsOutsideIntervals()
		{
			var today = DateOnly.Today;
			var otherPerson = PersonFactory.CreatePerson();
			var site = SiteFactory.CreateSiteWithOneTeam();

			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
			otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("test", null, null, null, null), person);
			organisationMembership = (OrganisationMembership)principal.Organisation;
			authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));

			PrepareClaims();

			using (mocks.Record())
			{
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today, otherPerson)).Return(true);
			}
			using (mocks.Playback())
			{
				var result = authorization.PermittedPeriods(applicationFunction.FunctionPath,
																	 new DateOnlyPeriod(today,
																						today.AddDays(2)), otherPerson);
				result.Count().Should().Be.EqualTo(1);
				result.Contains(new DateOnlyPeriod(today, today.AddDays(2))).Should().Be.True();
			}
		}

        private void PrepareClaims()
        {
            principal.AddClaimSet(
                new DefaultClaimSet(new[]
                                        {
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + Function, 
													applicationFunction,
                                                Rights.PossessProperty),
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/AvailableData",
                                                authorizeAvailableData, Rights.PossessProperty)
                                        }));
        }

        [Test]
        public void ShouldReturnGrantedFunctions()
        {
            organisationMembership.InitializeFromPerson(person);
			var result = authorization.GrantedFunctions();
            result.Count().Should().Be.EqualTo(1);
        }
		
        [Test]
        public void ShouldEvaluateClaimSetsWithSpecification()
        {
            var specification = mocks.StrictMock<ISpecification<IEnumerable<ClaimSet>>>();
            using (mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(null)).IgnoreArguments().Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.InitializeFromPerson(person);
                var result = authorization.EvaluateSpecification(specification);
                result.Should().Be.True();
            }
        }
    }
}
