using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Matrix;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Matrix
{
    [TestFixture]
    public class MatrixNavigationModelTest
    {
        private MatrixNavigationModel _target;
        private MockRepository _mocks;
	    private ILicenseActivator _licenseActivator;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new MatrixNavigationModel(() => "http://MatrixWebSiteUrl/");
				_licenseActivator = _mocks.DynamicMock<ILicenseActivator>();
				DefinedLicenseDataFactory.SetLicenseActivator("for test", _licenseActivator);
        }

        [Test]
        public void ShouldProvideAuthenticationType()
        {
            var expectedAuthenticationTypeOption = StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption;
            AuthenticationTypeOption actualAuthenticationTypeOption = _target.AuthenticationType;
            Assert.That(actualAuthenticationTypeOption, Is.EqualTo(expectedAuthenticationTypeOption));
        }

        [Test]
        public void ShouldProvideBusinessUnitId()
        {
            var expectedBusinessUnitId = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id;
            var actualBusinessUnitId = _target.BusinessUnitId;
            Assert.That(expectedBusinessUnitId, Is.EqualTo(actualBusinessUnitId));
        }

        [Test]
        public void ShouldProvideMatrixWebsiteUrl()
        {
            var expectedMatrixWebsiteUrl = "http://MatrixWebSiteUrl/?ReportID={0}&forceformslogin={1}&buid={2}";
            var actualMatrixWebsiteUrl = _target.MatrixWebsiteUrl;
            Assert.That(expectedMatrixWebsiteUrl, Is.EqualTo(actualMatrixWebsiteUrl));
        }

        [Test]
        public void ShouldProvidePermittedMatrixFunctions()
        {
            var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
            var expectedMatrixFunctions = new List<IApplicationFunction>();
            IEnumerable<IApplicationFunction> actualMatrixFunctions;
            using (_mocks.Record())
            {
                Expect.Call(authorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(expectedMatrixFunctions);
            }
            using (_mocks.Playback())
            {
                using(new CustomAuthorizationContext(authorization))
                {
                    actualMatrixFunctions = _target.PermittedMatrixFunctions;
                }
            }
            Assert.That(actualMatrixFunctions, Is.SameAs(expectedMatrixFunctions));
        }

        [Test]
        public void ShouldProvidePermittedOnlineReportsFunctions()
        {
            var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
            var expectedFunctions = new List<IApplicationFunction>();
            IEnumerable<IApplicationFunction> acctualFunctions;
            using (_mocks.Record())
            {
                Expect.Call(authorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(expectedFunctions);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    acctualFunctions = _target.PermittedOnlineReportFunctions;
                }
            }
            Assert.AreEqual(expectedFunctions, acctualFunctions);
        }

        [Test]
        public void ShouldProvideGroupedPermittedMatrixFunctions()
        {
            var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
            var matrixFunctions = new List<IApplicationFunction>
                                      {
                                          new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54"},
                                          new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup"}
                                      };
            IEnumerable<IMatrixFunctionGroup> actualMatrixFunctionGroups;
            using (_mocks.Record())
            {
                Expect.Call(authorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(matrixFunctions).Repeat.AtLeastOnce();
					 Expect.Call(_licenseActivator.EnabledLicenseOptionPaths).Return(new List<string>());
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    actualMatrixFunctionGroups = _target.GroupedPermittedMatrixFunctions;
                    Assert.That(actualMatrixFunctionGroups.Count(), Is.EqualTo(1));
                    Assert.That(actualMatrixFunctionGroups.ElementAt(0).ApplicationFunctions.Single(),
                                Is.SameAs(matrixFunctions.First()));
                }
            }
        }

        [Test]
        public void ShouldProvideOrphanPermittedMatrixFunctions()
        {

            var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
            var matrixFunctions = new List<IApplicationFunction>
                                      {
                                          new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54"},
                                          new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup"}
                                      };
            IEnumerable<IApplicationFunction> orphanMatrixFunctions;
            using (_mocks.Record())
            {
                Expect.Call(authorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(matrixFunctions).Repeat.AtLeastOnce();
	            Expect.Call(_licenseActivator.EnabledLicenseOptionPaths).Return(new List<string>());
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    orphanMatrixFunctions = _target.OrphanPermittedMatrixFunctions;

                    Assert.That(orphanMatrixFunctions.Count(), Is.EqualTo(1));
                    Assert.That(orphanMatrixFunctions.Single(), Is.SameAs(matrixFunctions.ElementAt(1)));
                }
            }
        }
    }
}