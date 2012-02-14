using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Claims;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class AbsenceAllowancePermissionSpecificationTest
    {
        private ISpecification<IEnumerable<ClaimSet>> target;

        [SetUp]
        public void Setup()
        {
            target = new AbsenceAllowancePermissionSpecification();
        }

        [Test]
        public void ShouldPassIfEnoughPermissionAllowed()
        {
            var claimSets = new DefaultClaimSet(new[]
                                        {
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + DefinedRaptorApplicationFunctionPaths.CreateAbsenceRequest, null,
                                                Rights.PossessProperty),
                                                new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, null,
                                                Rights.PossessProperty),
                                                new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + DefinedRaptorApplicationFunctionPaths.AbsenceRequests, null,
                                                Rights.PossessProperty)
                                        });
            Assert.IsTrue(target.IsSatisfiedBy(new[] {claimSets}));
        }

        [Test]
        public void ShouldFailIfNotEnoughPermissionAllowed()
        {
            var claimSets = new DefaultClaimSet(new[]
                                        {
                                            new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + DefinedRaptorApplicationFunctionPaths.CreateAbsenceRequest, null,
                                                Rights.PossessProperty),
                                                new Claim(
                                                TeleoptiAuthenticationHeaderNames.
                                                    TeleoptiAuthenticationHeaderNamespace + "/" + DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, null,
                                                Rights.PossessProperty)
                                        });
            Assert.IsFalse(target.IsSatisfiedBy(new[] { claimSets }));
        }
    }
}
