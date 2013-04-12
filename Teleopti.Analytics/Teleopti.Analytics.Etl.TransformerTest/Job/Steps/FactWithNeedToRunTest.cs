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
	[TestFixture]
	public class FactWithNeedRunTest
	{
		private MockRepository _mocks;
		private IChangedDataChecker _changedData;
		private IRaptorRepository _raptorRepository;
		private IJobParameters _jobParameters;
		private IBusinessUnit _bu;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_changedData = _mocks.DynamicMock<IChangedDataChecker>();
			_bu = _mocks.DynamicMock<IBusinessUnit>();
			_raptorRepository = _mocks.StrictMock<IRaptorRepository>();
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(_raptorRepository, null, null);
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckNeedRunOnSchedule()
		{
			Expect.Call(_changedData.NeedToRun(new DateTimePeriod(2011, 1, 1, 2011, 1, 2), _raptorRepository, _bu, ""))
				.IgnoreArguments()
				.Return(false);

			var step = new FactScheduleJobStep(_jobParameters, _changedData);
			_mocks.ReplayAll();
			step.Run(new List<IJobStep>(), _bu, null, false);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckNeedRunOnScheduleDays()
		{
			Expect.Call(_changedData.NeedToRun(new DateTimePeriod(2011, 1, 1, 2011, 1, 2), _raptorRepository, _bu, ""))
				.IgnoreArguments()
				.Return(false);

			var step = new FactScheduleDayCountJobStep(_jobParameters, _changedData);
			_mocks.ReplayAll();
			step.Run(new List<IJobStep>(), _bu, null, false);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckNeedRunOnPreference()
		{
			Expect.Call(_changedData.NeedToRun(new DateTimePeriod(2011, 1, 1, 2011, 1, 2), _raptorRepository, _bu, ""))
				.IgnoreArguments()
				.Return(false);

			var step = new FactSchedulePreferenceJobStep(_jobParameters, _changedData);
			_mocks.ReplayAll();
			step.Run(new List<IJobStep>(), _bu, null, false);
			_mocks.VerifyAll();
		}
	}

	
}