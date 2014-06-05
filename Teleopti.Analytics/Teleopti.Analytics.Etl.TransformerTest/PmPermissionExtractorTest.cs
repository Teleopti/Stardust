using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class PmPermissionExtractorTest
    {
        private PmPermissionExtractor _target;
        private IEnumerable<IApplicationFunction> _applicationFunctionList;
        private MockRepository _mocks;
        private ILicensedFunctionsProvider _licensedFunctionsProvider;
	    private IUnitOfWorkFactory _unitOfWorkFactory;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _licensedFunctionsProvider = _mocks.StrictMock<ILicensedFunctionsProvider>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _applicationFunctionList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList;
            _target = new PmPermissionExtractor(_licensedFunctionsProvider);
        }

        [Test]
        public void ShouldOnlyCheckLicenseWithAuthorizationServiceOncePerInstance()
        {
            mockLicensedApplicationFunctions(true, true);

            bool isPmLicenseValid1 = _target.IsPerformanceManagerLicenseValid(_unitOfWorkFactory);
            bool isPmLicenseValid2 = _target.IsPerformanceManagerLicenseValid(_unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.IsTrue(isPmLicenseValid1);
            Assert.IsTrue(isPmLicenseValid2);
        }

        [Test]
        public void ShouldExtractNoPermissionsCauseNoLicense()
        {
            mockLicensedApplicationFunctions(false, false);
            PmPermissionType pmPermission = _target.ExtractPermission(new List<IApplicationFunction> { getApplicationFunction("View") },_unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(PmPermissionType.None, pmPermission);
        }

        [Test]
        public void ShouldExtractNoPermissions()
        {
            mockLicensedApplicationFunctions(true, false);
            PmPermissionType pmPermission = _target.ExtractPermission(new List<IApplicationFunction>(),_unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(PmPermissionType.None, pmPermission);
        }

        [Test]
        public void ShouldExtractViewPermissions()
        {
            mockLicensedApplicationFunctions(true, false);
            PmPermissionType pmPermission = _target.ExtractPermission(new List<IApplicationFunction> { getApplicationFunction("View") },_unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(PmPermissionType.GeneralUser, pmPermission);
        }

        [Test]
        public void ShouldExtractCreatePermissions()
        {
            mockLicensedApplicationFunctions(true, false);
            PmPermissionType pmPermission = _target.ExtractPermission(new List<IApplicationFunction> { getApplicationFunction("Create") },_unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(PmPermissionType.ReportDesigner, pmPermission);
        }

        [Test]
        public void ShouldExtractCreatePermissionWhenApplicationFunctionAllIsProvided()
        {
            mockLicensedApplicationFunctions(true, false);
            PmPermissionType pmPermission = _target.ExtractPermission(new List<IApplicationFunction> { getApplicationFunction("All") },_unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(PmPermissionType.ReportDesigner, pmPermission);
        }

        private void mockLicensedApplicationFunctions(bool isPmLicensed, bool doCheckLicenseRepeatOnce)
        {
            var licensedApplicationFunctions = new List<IApplicationFunction>();
            if (isPmLicensed)
            {
                licensedApplicationFunctions.Add(getApplicationFunction("View"));
                licensedApplicationFunctions.Add(getApplicationFunction("Create"));
            }

	        Expect.Call(_unitOfWorkFactory.Name).Return("for test");
            if (doCheckLicenseRepeatOnce)
                Expect.Call(_licensedFunctionsProvider.LicensedFunctions("for test")).Return(licensedApplicationFunctions).Repeat.Once();
            else
                Expect.Call(_licensedFunctionsProvider.LicensedFunctions("for test")).Return(licensedApplicationFunctions).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
        }

        private IApplicationFunction getApplicationFunction(string function)
        {
            switch (function)
            {
                case "View":
                    return
                        _applicationFunctionList.Where(
                            a => a.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ViewPerformanceManagerReport)
                            .First();
                case "Create":
                    return
                        _applicationFunctionList.Where(
                            a =>
                            a.ForeignId == DefinedRaptorApplicationFunctionForeignIds.CreatePerformanceManagerReport).
                            First();
                case "All":
                    return _applicationFunctionList.Where(
                            a => a.ForeignId == DefinedRaptorApplicationFunctionForeignIds.All)
                            .First();
            }

            return null;
        }
    }
}
