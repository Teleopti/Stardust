using System;
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
	public class FactIntradayRequestJobStepTest
	{
		private MockRepository _mocks;
		private FactRequestJobStep _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();

		}

		[Test]
		public void ShouldCallAnotherIfIntraday()
		{
			var raptorRepository = _mocks.StrictMock<IRaptorRepository>();

			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
		
			_target = new FactRequestJobStep(jobParameters, true);

			Expect.Call(raptorRepository.FillIntradayFactRequestMart(null)).Return(5).IgnoreArguments();
			
			_mocks.ReplayAll();
			_target.Run(new List<IJobStep>(), null, null, true);
			_mocks.VerifyAll();
		}

		[Test]
		public void DayShouldCallAnotherIfIntraday()
		{
			var raptorRepository = _mocks.StrictMock<IRaptorRepository>();
			var commonStateHolder = _mocks.StrictMock<ICommonStateHolder>();
			
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);

			
			var target = new FactRequestedDaysJobStep(jobParameters, true);

			Expect.Call(raptorRepository.FillIntradayFactRequestedDaysMart(null)).Return(5).IgnoreArguments();
			Expect.Call(() => commonStateHolder.UpdateThisTime("Request", null)).IgnoreArguments();
			_mocks.ReplayAll();
			target.Run(new List<IJobStep>(), null, null, true);
			_mocks.VerifyAll();
		}
	}

	
}