using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class IntradayStageRequestJobStepTest
	{
		private IntradayStageRequestJobStep _target;
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = MockRepository.GenerateMock<IJobParameters>();
			_target = new IntradayStageRequestJobStep(_jobParameters);

		}

		[Test]
		public void ShouldRunStep_SameTime()
		{
			var raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var lastChanged = new LastChangedReadModel
				{
					LastTime = new DateTime(2013, 05, 03),
					ThisTime = new DateTime(2013, 05, 03)
				};
			
			_jobParameters.Expect(j => j.Helper).Return(jobHelper);
			jobHelper.Expect(j => j.Repository).Return(raptorRepository);
			raptorRepository.Expect(j => j.LastChangedDate(new BusinessUnit("TestBu"), "Step")).IgnoreArguments()
			                .Return(lastChanged);
			_jobParameters.Expect(j => j.StateHolder).Return(stateHolder);
			stateHolder.Expect(s => s.SetThisTime(lastChanged, "Step"));
			raptorRepository.Expect(r => r.TruncateRequest());

			var result = _target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			result.RowsAffected.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRunStep_DifferentTime()
		{
			var raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var lastChanged = new LastChangedReadModel
			{
				LastTime = new DateTime(2013, 05, 03),
				ThisTime = new DateTime(2013, 05, 04)
			};

			_jobParameters.Expect(j => j.Helper).Return(jobHelper);
			jobHelper.Expect(j => j.Repository).Return(raptorRepository);
			raptorRepository.Expect(j => j.LastChangedDate(new BusinessUnit("TestBu"), "Step")).IgnoreArguments()
						  .Return(lastChanged);
			_jobParameters.Expect(j => j.StateHolder).Return(stateHolder);
			stateHolder.Expect(s => s.SetThisTime(lastChanged, "Step"));
			stateHolder.Expect(s => s.PersonCollection).IgnoreArguments().Return(new List<IPerson>());
			raptorRepository.Expect(r => r.LoadIntradayRequest(new DateTime())).IgnoreArguments().Return(new List<IPersonRequest>());

			var result = _target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			result.RowsAffected.Should().Be.EqualTo(0);
		}
	}
}
