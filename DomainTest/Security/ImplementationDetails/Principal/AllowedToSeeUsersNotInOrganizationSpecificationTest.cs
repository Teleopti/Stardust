using System.Collections.Generic;
using System.IdentityModel.Claims;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class AllowedToSeeUsersNotInOrganizationSpecificationTest
    {
        private ISpecification<IEnumerable<ClaimSet>> target;
        private const string Function = "test";

        [SetUp]
        public void Setup()
        {
            target = new AllowedToSeeUsersNotInOrganizationSpecification(Function);
        }

        [Test]
        public void ShouldAllowClaimWithEveryoneToSeePeople()
        {
            var claimSet = PrepareClaimSet(new AuthorizeEveryone());
            target.IsSatisfiedBy(new [] {claimSet}).Should().Be.True();
        }

        [Test]
        public void ShouldAllowClaimWithBusinessUnitToSeePeople()
        {
            var claimSet = PrepareClaimSet(new AuthorizeMyBusinessUnit());
            target.IsSatisfiedBy(new[] { claimSet }).Should().Be.True();
        }

        [Test]
        public void ShouldNotAllowClaimWithSiteToSeePeople()
        {
            var claimSet = PrepareClaimSet(new AuthorizeMySite());
            target.IsSatisfiedBy(new[] { claimSet }).Should().Be.False();
        }

        [Test]
        public void ShouldAllowClaimWithCurrentBusinessUnitToSeePeople()
        {
            var availableData = new AvailableData();
            availableData.AddAvailableBusinessUnit(((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit);
            var claimSet = PrepareClaimSet(AuthorizeExternalAvailableData.Create(availableData));
            target.IsSatisfiedBy(new[] { claimSet }).Should().Be.True();
        }

        private static ClaimSet PrepareClaimSet(IAuthorizeAvailableData authorizeAvailableData)
        {
            return new DefaultClaimSet(new[]
                                        {
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + Function, null,
                                                Rights.PossessProperty),
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/AvailableData",
                                                authorizeAvailableData, Rights.PossessProperty)
                                        });
        }
    }
}
