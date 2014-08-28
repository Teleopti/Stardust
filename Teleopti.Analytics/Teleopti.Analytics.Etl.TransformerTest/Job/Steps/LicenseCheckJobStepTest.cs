using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class LicenseCheckJobStepTest
	{
		private MockRepository _mocks;
		private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
		private IRaptorRepository _raptorRepository;
		private LicenseCheckJobStep _target;
		private ILicenseStatusUpdater _licenseUpdater;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_raptorRepository = _mocks.StrictMock<IRaptorRepository>();
			_licenseUpdater = _mocks.StrictMock<ILicenseStatusUpdater>();
			_jobParameters.Helper = new JobHelper(_raptorRepository, null, null, null);
			_target = new LicenseCheckJobStep(_jobParameters);
		}

		[Test]
		public void ShouldRunLicenseStatusUpdater()
		{
			Expect.Call(_raptorRepository.LicenseStatusUpdater).Return(_licenseUpdater);
			Expect.Call(_licenseUpdater.RunCheck()).Return(1);
			_mocks.ReplayAll();
			_target.Run(new List<IJobStep>(),null, null, false);
			_mocks.VerifyAll();   
		}

		

	}

	
}