using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class LicenseCheckJobStepTest
    {
        private MockRepository _mocks;
        private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
        private IRaptorRepository _raptorRepository;
        private LicenseCheckJobStep _target;
        private ILicenseStatusXml _licenseStatus;
        private ILicenseService _licenseService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _raptorRepository = _mocks.StrictMock<IRaptorRepository>();
            _licenseStatus = _mocks.StrictMock<ILicenseStatusXml>();
            _licenseService = _mocks.StrictMock<ILicenseService>();
            _jobParameters.Helper = new JobHelper(_raptorRepository, null, null);
            _target = new LicenseCheckJobStep(_jobParameters);
        }

        [Test]
        public void ShouldCheckOkLicenseUsage()
        {
            Expect.Call(_raptorRepository.NumberOfActiveAgents()).Return(500);
            Expect.Call(_raptorRepository.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_raptorRepository.XmlLicenseService(500)).Return(_licenseService);
            
            Expect.Call(_licenseStatus.CheckDate = DateTime.Today.Date);
            Expect.Call(_licenseStatus.LastValidDate = DateTime.Today.AddDays(1));
            Expect.Call(_licenseStatus.StatusOk = true);
            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseService.IsThisAlmostTooManyActiveAgents(500)).Return(true);
            Expect.Call(_licenseStatus.AlmostTooMany = true);
            Expect.Call(_licenseStatus.XmlDocument).Return(new XmlDocument());
            Expect.Call(() =>_raptorRepository.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.Run(new List<IJobStep>(),null, null, false);
            _mocks.VerifyAll();   
        }

        [Test]
        public void ShouldCheckNotOkLicenseUsage()
        {
            Expect.Call(_raptorRepository.NumberOfActiveAgents()).Return(500);
            Expect.Call(_raptorRepository.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_raptorRepository.XmlLicenseService(500)).Throw(new TooManyActiveAgentsException());
            
            Expect.Call(_licenseStatus.StatusOk).Return(true);

            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseStatus.CheckDate = DateTime.Today);
            Expect.Call(_licenseStatus.LastValidDate = DateTime.Today.AddDays(30));
            Expect.Call(_licenseStatus.StatusOk = false);
            Expect.Call(_licenseStatus.DaysLeft = 30);
            Expect.Call(_licenseStatus.AlmostTooMany = false);
            Expect.Call(_licenseStatus.XmlDocument).Return(new XmlDocument());
            Expect.Call(() => _raptorRepository.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.Run(new List<IJobStep>(), null, null, false);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCheckNotOkAndNotOkBeforeLicenseUsage()
        {
            Expect.Call(_raptorRepository.NumberOfActiveAgents()).Return(500);
            Expect.Call(_raptorRepository.LicenseStatus).Return(_licenseStatus);
            Expect.Call(_raptorRepository.XmlLicenseService(500)).Throw(new TooManyActiveAgentsException());

            Expect.Call(_licenseStatus.StatusOk).Return(false).Repeat.Times(2);
            Expect.Call(_licenseStatus.NumberOfActiveAgents = 500);
            Expect.Call(_licenseStatus.CheckDate = DateTime.Today.Date);
            Expect.Call(_licenseStatus.LastValidDate).Return(DateTime.Today.AddDays(21).Date);
            Expect.Call(_licenseStatus.DaysLeft = 20);
            Expect.Call(_licenseStatus.XmlDocument).Return(new XmlDocument());
            Expect.Call(() => _raptorRepository.SaveLicenseStatus(""));
            _mocks.ReplayAll();
            _target.Run(new List<IJobStep>(), null, null, false);
            _mocks.VerifyAll();
        }

    }

    
}