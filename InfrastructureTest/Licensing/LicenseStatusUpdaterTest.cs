using System;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    public class LicenseStatusUpdaterTest
    {
        private MockRepository _mocks;
        private LicenseStatusUpdater _target;
        private ILicenseStatusRepositories _licenseStatusReps;
        private ILicenseStatusXml _licenseStatus;
        private ILicenseService _licenseService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _licenseStatusReps = _mocks.StrictMock<ILicenseStatusRepositories>();
            _licenseStatus = _mocks.StrictMock<ILicenseStatusXml>();
            _licenseService = _mocks.StrictMock<ILicenseService>();
            _target = new LicenseStatusUpdater(_licenseStatusReps);
        }

        [Test]
        public void ShouldCheckOkLicenseUsage()
        {
            Expect.Call(_licenseStatusReps.NumberOfActiveAgents()).Return(500);
            Expect.Call(_licenseStatusReps.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_licenseStatusReps.XmlLicenseService(500)).Return(_licenseService);

            Expect.Call(_licenseStatus.CheckDate = DateTime.Today.Date);
            Expect.Call(_licenseStatus.LastValidDate = DateTime.Today.AddDays(1));
            Expect.Call(_licenseStatus.StatusOk = true);
            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseService.IsThisAlmostTooManyActiveAgents(500)).Return(true);
            Expect.Call(_licenseStatus.AlmostTooMany = true);
            Expect.Call(_licenseStatus.GetNewStatusDocument()).Return(new XmlDocument());
            Expect.Call(() => _licenseStatusReps.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.RunCheck();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCheckNotOkLicenseUsage()
        {
            Expect.Call(_licenseStatusReps.NumberOfActiveAgents()).Return(500);
            Expect.Call(_licenseStatusReps.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_licenseStatusReps.XmlLicenseService(500)).Throw(new TooManyActiveAgentsException());

            Expect.Call(_licenseStatus.StatusOk).Return(true);

            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseStatus.CheckDate = DateTime.Today);
            Expect.Call(_licenseStatus.LastValidDate = DateTime.Today.AddDays(30));
            Expect.Call(_licenseStatus.StatusOk = false);
            Expect.Call(_licenseStatus.DaysLeft = 30);
            Expect.Call(_licenseStatus.AlmostTooMany = false);
            Expect.Call(_licenseStatus.GetNewStatusDocument()).Return(new XmlDocument());
            Expect.Call(() => _licenseStatusReps.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.RunCheck();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCheckNotOkAndNotOkBeforeLicenseUsage()
        {
            Expect.Call(_licenseStatusReps.NumberOfActiveAgents()).Return(500);
            Expect.Call(_licenseStatusReps.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_licenseStatusReps.XmlLicenseService(500)).Throw(new TooManyActiveAgentsException());

            Expect.Call(_licenseStatus.StatusOk).Return(false).Repeat.Times(2);
            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseStatus.CheckDate = DateTime.Today.Date);
            Expect.Call(_licenseStatus.LastValidDate).Return(DateTime.Today.AddDays(21).Date);
            Expect.Call(_licenseStatus.DaysLeft = 20);
            Expect.Call(_licenseStatus.GetNewStatusDocument()).Return(new XmlDocument());
            Expect.Call(() => _licenseStatusReps.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.RunCheck();
            _mocks.VerifyAll();
        }
    } 
}