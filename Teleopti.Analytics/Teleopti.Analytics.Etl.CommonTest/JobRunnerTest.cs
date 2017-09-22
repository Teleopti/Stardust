using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Toggle;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using JobResult = Teleopti.Analytics.Etl.Common.Transformer.Job.JobResult;

namespace Teleopti.Analytics.Etl.CommonTest
{
	[TestFixture]
	public class JobRunnerTest
	{
		private JobRunner _target;

		[Test]
		public void ShouldRunJobForOneBusinessUnit()
		{
			var job = MockRepository.GenerateMock<IJob>();

			IBusinessUnit businessUnit = new BusinessUnit("myBU");
			var businessUnits = new List<IBusinessUnit> { businessUnit };
			var parameters = MockRepository.GenerateMock<IJobParameters>();
			parameters.Stub(x => x.ToggleManager).Return(new FakeToggleManager());
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();

			job.Stub(x => x.JobParameters).Return(parameters);
			parameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.BusinessUnitCollection).Return(businessUnits);
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			IJobResult jobResult = new JobResult(businessUnit, jobResultCollection);

			job.Stub(x => x.Run(businessUnit, new List<IJobStep>(), new List<IJobResult>(), true, true)).Return(jobResult);

			_target = new JobRunner();
			var expectedJobResultCollection = _target.Run(job, jobResultCollection, jobStepsNotToRun);

			expectedJobResultCollection.Count.Should().Be.EqualTo(1);
			expectedJobResultCollection[0].Should().Be.SameInstanceAs(jobResult);
		}

		[Test]
		public void ShouldRunJobForTwoBusinessUnits()
		{
			var job = MockRepository.GenerateMock<IJob>();

			IBusinessUnit businessUnit1 = new BusinessUnit("myBU 1");
			IBusinessUnit businessUnit2 = new BusinessUnit("myBU 2");
			IList<IBusinessUnit> businessUnits = new List<IBusinessUnit>
			{
				businessUnit1,
				businessUnit2
			};
			var parameters = MockRepository.GenerateMock<IJobParameters>();
			parameters.Stub(x => x.ToggleManager).Return(new FakeToggleManager());
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();

			job.Stub(x => x.JobParameters).Return(parameters);
			parameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.BusinessUnitCollection).Return(businessUnits);
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			IJobResult jobResult1 = new JobResult(businessUnit1, jobResultCollection);
			IJobResult jobResult2 = new JobResult(businessUnit2, jobResultCollection);

			job.Stub(x => x.Run(businessUnit1, jobStepsNotToRun, jobResultCollection, true, false)).Return(jobResult1);
			job.Stub(x => x.Run(businessUnit2, jobStepsNotToRun, jobResultCollection, false, true)).Return(jobResult2);

			_target = new JobRunner();
			var expectedJobResultCollection = _target.Run(job, jobResultCollection, jobStepsNotToRun);

			expectedJobResultCollection.Count.Should().Be.EqualTo(2);
			expectedJobResultCollection[0].Should().Be.SameInstanceAs(jobResult1);
			expectedJobResultCollection[1].Should().Be.SameInstanceAs(jobResult2);
		}

		[Test]
		public void ShouldSaveJobResultForOneBusinessUnit()
		{
			var repository = MockRepository.GenerateMock<IJobLogRepository>();
			IBusinessUnit businessUnit1 = new BusinessUnit("myBU 1");
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IJobResult jobResult = new JobResult(businessUnit1, jobResultCollection);
			IJobStepResult jobStepResult = new JobStepResult("step1", 10d, null, businessUnit1, jobResultCollection);
			jobResult.JobStepResultCollection.Add(jobStepResult);
			jobResultCollection.Add(jobResult);

			repository.Expect(x => x.SaveLogPre()).Return(99);
			repository.Expect(x => x.SaveLogStepPost(Arg<EtlJobLog>.Is.Anything, Arg<JobStepResult>.Matches(arg=> arg == jobStepResult)));
			repository.Expect(x => x.SaveLogPost(Arg<EtlJobLog>.Is.Anything, Arg<JobResult>.Matches(arg=> arg == jobResult)));
			
			_target = new JobRunner();
			_target.SaveResult(jobResultCollection, repository, -1);

			repository.VerifyAllExpectations();
		}

		[Test]
		public void ShouldSaveJobResultForTwoBusinessUnits()
		{
			var repository = MockRepository.GenerateMock<IJobLogRepository>();
			IBusinessUnit businessUnit1 = new BusinessUnit("myBU 1");
			IBusinessUnit businessUnit2 = new BusinessUnit("myBU 2");
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IJobResult jobResult1 = new JobResult(businessUnit1, jobResultCollection);
			IJobStepResult jobStepResult1 = new JobStepResult("step1", 10d, null, businessUnit1, jobResultCollection);
			jobResult1.JobStepResultCollection.Add(jobStepResult1);
			IJobResult jobResult2 = new JobResult(businessUnit2, jobResultCollection);
			IJobStepResult jobStepResult2 = new JobStepResult("step2", 10d, null, businessUnit2, jobResultCollection);
			jobResult2.JobStepResultCollection.Add(jobStepResult2);

			jobResultCollection.Add(jobResult1);
			jobResultCollection.Add(jobResult2);

			repository.Expect(x => x.SaveLogPre()).Return(99).Repeat.Once();
			repository.Expect(x => x.SaveLogStepPost(Arg<EtlJobLog>.Is.Anything, Arg<JobStepResult>.Matches(arg => arg == jobStepResult1))).Repeat.Once();
			repository.Expect(x => x.SaveLogPost(Arg<EtlJobLog>.Is.Anything, Arg<JobResult>.Matches(arg => arg == jobResult1))).Repeat.Once();
			repository.Expect(x => x.SaveLogPre()).Return(100).Repeat.Once();
			repository.Expect(x => x.SaveLogStepPost(Arg<EtlJobLog>.Is.Anything, Arg<JobStepResult>.Matches(arg => arg == jobStepResult2))).Repeat.Once();
			repository.Expect(x => x.SaveLogPost(Arg<EtlJobLog>.Is.Anything, Arg<JobResult>.Matches(arg => arg == jobResult2))).Repeat.Once();

			_target = new JobRunner();
			_target.SaveResult(jobResultCollection, repository, -1);

			repository.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnNullWhenJobRunReturnsNull()
		{
			var job = MockRepository.GenerateMock<IJob>();
			IBusinessUnit businessUnit = new BusinessUnit("myBU");
			var businessUnits = new List<IBusinessUnit> { businessUnit };
			var parameters = MockRepository.GenerateMock<IJobParameters>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();

			job.Stub(x => x.JobParameters).Return(parameters);
			parameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.BusinessUnitCollection).Return(businessUnits);
			job.Stub(x => x.Run(businessUnit, new List<IJobStep>(), new List<IJobResult>(), true, true)).Return(null);

			_target = new JobRunner();
			var expectedJobResultCollection = _target.Run(job, new List<IJobResult>(), new List<IJobStep>());

			expectedJobResultCollection.Should().Be.Null();
		}
	}
}
