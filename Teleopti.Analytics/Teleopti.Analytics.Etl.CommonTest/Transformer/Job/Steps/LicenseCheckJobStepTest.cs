using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
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
			_jobParameters.Helper = new JobHelperForTest(_raptorRepository, null, null);
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