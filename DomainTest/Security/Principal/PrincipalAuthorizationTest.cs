﻿using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class PrincipalAuthorizationTest
    {
        private IPrincipalAuthorization principalAuthorization;
        private IPerson person;
        private ITeleoptiPrincipal principal;
        private MockRepository mocks;
        private IAuthorizeAvailableData authorizeAvailableData;
        private IApplicationFunction applicationFunction;
        private IOrganisationMembership organisationMembership;
        const string Function = "test";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            authorizeAvailableData = mocks.StrictMock<IAuthorizeAvailableData>();
            person = PersonFactory.CreatePerson();

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
            organisationMembership = principal.Organisation;
            principalAuthorization = new PrincipalAuthorization(principal);
            applicationFunction = mocks.StrictMock<IApplicationFunction>();
            
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
                organisationMembership.AddFromPerson(person);
                Assert.IsTrue(principalAuthorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldCheckPermissionsForFunction()
        {
            organisationMembership.AddFromPerson(person);
            Assert.IsTrue(principalAuthorization.IsPermitted(Function));
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
                organisationMembership.AddFromPerson(person);
                Assert.IsTrue(principalAuthorization.IsPermitted(Function, DateOnly.Today, toCheck));
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
                organisationMembership.AddFromPerson(person);
                Assert.IsTrue(principalAuthorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldCheckPermissionsForBusinessUnit()
        {
            var toCheck = mocks.StrictMock<IBusinessUnit>();
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, DateOnly.Today, toCheck)).Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.AddFromPerson(person);
                Assert.IsTrue(principalAuthorization.IsPermitted(Function, DateOnly.Today, toCheck));
            }
        }

        [Test]
        public void ShouldHandlePermittedPeriodsCorrectly()
        {
            applicationFunction = new ApplicationFunction(Function);
            var today = DateOnly.Today;
            var otherPerson = PersonFactory.CreatePerson();
            var site = SiteFactory.CreateSiteWithOneTeam();

            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10),site.TeamCollection[0]));
            otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
            organisationMembership = principal.Organisation;
            principalAuthorization = new PrincipalAuthorization(principal);

            PrepareClaims();
            
            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-20), otherPerson)).Return(true);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(false);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-10), otherPerson)).Return(true);
            }
            using (mocks.Playback())
            {
                var result = principalAuthorization.PermittedPeriods(applicationFunction,
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
            applicationFunction = new ApplicationFunction(Function);
            var today = DateOnly.Today;
            var otherPerson = PersonFactory.CreatePerson();
            var site = SiteFactory.CreateSiteWithOneTeam();

            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
            otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
            organisationMembership = principal.Organisation;
            principalAuthorization = new PrincipalAuthorization(principal);

            PrepareClaims();

            using (mocks.Record())
            {
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-20), otherPerson)).Return(true);
                Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(false);
            }
            using (mocks.Playback())
            {
                var result = principalAuthorization.PermittedPeriods(applicationFunction,
                                                                     new DateOnlyPeriod(today.AddDays(-20),
                                                                                        today.AddDays(-12)), otherPerson);
                result.Count().Should().Be.EqualTo(1);
                result.Contains(new DateOnlyPeriod(today.AddDays(-20), today.AddDays(-16))).Should().Be.True();
            }
        }

		[Test]
		public void ShouldHandleTerminalDateProperlyWhenGettingPermittedPeriods()
		{
			applicationFunction = new ApplicationFunction(Function);
			var today = DateOnly.Today;
			var otherPerson = PersonFactory.CreatePerson();
			var site = SiteFactory.CreateSiteWithOneTeam();

			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
			otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));
			person.TerminalDate = today;

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
			organisationMembership = principal.Organisation;
			principalAuthorization = new PrincipalAuthorization(principal);

			PrepareClaims();

			using (mocks.Record())
			{
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-15), otherPerson)).Return(true);
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today.AddDays(-10), otherPerson)).Return(true);
			}
			using (mocks.Playback())
			{
				var result = principalAuthorization.PermittedPeriods(applicationFunction,
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
			applicationFunction = new ApplicationFunction(Function);
			var today = DateOnly.Today;
			var otherPerson = PersonFactory.CreatePerson();
			var site = SiteFactory.CreateSiteWithOneTeam();

			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-10), site.TeamCollection[0]));
			otherPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(today.AddDays(-15), site.TeamCollection[0]));

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
			organisationMembership = principal.Organisation;
			principalAuthorization = new PrincipalAuthorization(principal);

			PrepareClaims();

			using (mocks.Record())
			{
				Expect.Call(authorizeAvailableData.Check(organisationMembership, today, otherPerson)).Return(true);
			}
			using (mocks.Playback())
			{
				var result = principalAuthorization.PermittedPeriods(applicationFunction,
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
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + Function, applicationFunction,
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
            organisationMembership.AddFromPerson(person);
            var result = principalAuthorization.GrantedFunctions();
            result.Count().Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldReturnGrantedFunctionsBasedOnSpecification()
        {
            ISpecification<IApplicationFunction> specification =
                mocks.StrictMock<ISpecification<IApplicationFunction>>();
            using (mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(applicationFunction)).Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.AddFromPerson(person);
                var result = principalAuthorization.GrantedFunctionsBySpecification(specification);
                result.Count().Should().Be.EqualTo(1);
            }
        }

        [Test]
        public void ShouldEvaluateClaimSetsWithSpecification()
        {
            ISpecification<IEnumerable<ClaimSet>> specification =
                mocks.StrictMock<ISpecification<IEnumerable<ClaimSet>>>();
            using (mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(null)).IgnoreArguments().Return(true);
            }
            using (mocks.Playback())
            {
                organisationMembership.AddFromPerson(person);
                var result = principalAuthorization.EvaluateSpecification(specification);
                result.Should().Be.True();
            }
        }
    }
}
