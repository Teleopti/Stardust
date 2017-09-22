using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
	[TestFixture]
	public class JobBaseTest
	{
		private JobBase _jobBase1;
		private JobBase _jobBase2;
		private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		private IJobParameters _jobParameters;
		private IJobHelper _jobHelper;
		private MockRepository _mocks;
		private IBusinessUnit _bu1;
		private IBusinessUnit _bu2;
		private IList<IJobStep> _jobStepList1;
		private IList<IJobStep> _jobStepList2;
		private IJobResult _jobResult1;
		private IJobResult _jobResult2;
		private IList<IJobStep> _jobStepsNotToRun;
		private string _jobName1;
		private string _jobName2;
		private string _jobStepName1;
		private string _jobStepName2;
		private string _jobStepErrorName;

		[SetUp]
		public void Setup()
		{
			_jobName1 = "Job 1";
			_jobName2 = "Job 2";
			_jobStepName1 = "JobStep 1";
			_jobStepName2 = "JobStep 2";
			_jobStepErrorName = "test_error_jobstep";
			_bu1 = new BusinessUnit("BU1");
			_bu2 = new BusinessUnit("BU2");
			_jobStepsNotToRun = new List<IJobStep>();

			_mocks = new MockRepository();
			_jobParameters = _mocks.StrictMock<IJobParameters>();
			_jobHelper = _mocks.StrictMock<IJobHelper>();
			Expect.Call(_jobParameters.Helper).Return(_jobHelper).Repeat.Any();

			// Create job 1
			_jobStepList1 = new List<IJobStep>();
			IJobStep jobStep = new TestJobStep(_jobParameters, _jobStepName1, true);
			_jobStepList1.Add(jobStep); // This jobstep should run
			_jobStepList1.Add(jobStep); // This jobstep should NOT run because it has already been run
			_jobStepList1.Add(new TestErrorJobStep(_jobParameters, _jobStepErrorName));    // This step should cause exception
			_jobBase1 = new JobBase(_jobParameters, _jobStepList1, _jobName1, false, false);

			// Log on to bu 1
			Expect.Call(_jobHelper.SetBusinessUnit(_bu1)).Return(true);
			_jobHelper.LogOffTeleoptiCccDomain();
			Expect.Call(_jobParameters.StateHolder).PropertyBehavior();
			_mocks.ReplayAll();

			// Run job 1
			_jobResult1 = _jobBase1.Run(_bu1, _jobStepsNotToRun, null, true, false);

			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyName()
		{
			Assert.AreEqual(_jobName1, _jobBase1.Name);
			Assert.AreEqual(_jobBase1.Name, _jobBase1.ToString());

			_jobBase1.Name = "changed job name";
			Assert.AreEqual("changed job name", _jobBase1.Name);
		}

		[Test]
		public void VerifyEnabled()
		{
			Assert.IsTrue(_jobBase1.Enabled);
			_jobBase1.Enabled = false;
			Assert.IsFalse(_jobBase1.Enabled);
		}

		[Test]
		public void VerifyGetJobCategoryCollection()
		{
			var dt1 = new DateTime(2006, 1, 1);
			var dt2 = new DateTime(2006, 1, 2);
			var dt3 = new DateTime(2006, 1, 3);
			var dt4 = new DateTime(2006, 1, 4);
			IJobMultipleDate jobMultipleDate =
				new JobMultipleDate(_timeZone);
			jobMultipleDate.Add(new JobMultipleDateItem(DateTimeKind.Local, dt1, dt2, _timeZone),
								JobCategoryType.Schedule);
			jobMultipleDate.Add(new JobMultipleDateItem(DateTimeKind.Local, dt3, dt4, _timeZone),
								JobCategoryType.Forecast);

			var jobParameters = new JobParameters(
				jobMultipleDate, 1, _timeZone.Id, 15,
				"Data Source=SSAS_Server;Initial Catalog=SSAS_DB", 
				"false", 
				CultureInfo.CurrentCulture, 
				new JobParametersFactory.FakeContainerHolder(), 
				false);
			IList<IJobStep> jobStepList = new List<IJobStep>();
			jobStepList.Add(new StageScheduleJobStep(jobParameters));
			jobStepList.Add(new StageForecastWorkloadJobStep(jobParameters));

			var jobBase = new JobBase(_jobParameters, jobStepList, _jobName1, true, false);

			Assert.AreEqual(2, jobBase.JobCategoryCollection.Count);
		}

		[Test]
		public void VerifyJobStepIsBusinessUnitIndependent()
		{
			Assert.IsNotNull(_jobResult1);
			Assert.IsNotNull(_jobResult1.JobStepResultCollection[0]);
			Assert.IsNotNull(_jobResult1.JobStepResultCollection[1]);

			IJobStepResult jobStepResult1 = _jobResult1.JobStepResultCollection[0];
			IJobStepResult jobStepResult2 = _jobResult1.JobStepResultCollection[1];

			Assert.AreEqual(_jobStepName1, jobStepResult1.Name);
			Assert.AreEqual("Done", jobStepResult1.Status);
			Assert.AreEqual(9, jobStepResult1.RowsAffected);
			Assert.GreaterOrEqual(jobStepResult1.Duration, 0d);
			
			Assert.AreEqual(_jobStepName1, jobStepResult2.Name);
			Assert.AreEqual("No need to run", jobStepResult2.Status);
			Assert.AreEqual(0, jobStepResult2.RowsAffected);
			Assert.AreEqual(0, jobStepResult2.Duration);
		}

		[Test]
		public void VerifyExceptionOnJobStep()
		{
			Assert.IsNotNull(_jobResult1.JobStepResultCollection[2]);
			IJobStepResult jobStepResult = _jobResult1.JobStepResultCollection[2];

			Assert.AreEqual("test_error_jobstep", jobStepResult.Name);
			Assert.AreEqual("Error", jobStepResult.Status);
			Assert.IsFalse(jobStepResult.RowsAffected.HasValue);
			Assert.GreaterOrEqual(jobStepResult.Duration, 0d);
			Assert.IsNotNull(jobStepResult.JobStepException);
			Assert.AreEqual("Expected error in test.", jobStepResult.JobStepException.Message);
		}

		[Test]
		public void VerifyJobResultErrorWhenMultipleBusinessUnit()
		{
			_jobParameters = _mocks.StrictMock<IJobParameters>();
			_jobHelper = _mocks.StrictMock<IJobHelper>();
			Expect.Call(_jobParameters.Helper).Return(_jobHelper).Repeat.Any();

			// Create job 2
			_jobStepList2 = new List<IJobStep>();
			IJobStep jobStep2 = new TestJobStep(_jobParameters, _jobStepName2, false);
			_jobStepList2.Add(jobStep2); // This jobstep should run
			// This step with same name in job 1 run throw exception. Should show error here in jobstepresult
			_jobStepList2.Add(new TestJobStep(_jobParameters, _jobStepErrorName, false));
			_jobBase2 = new JobBase(_jobParameters, _jobStepList2, _jobName2, false, false);

			using (_mocks.Record())
			{
				// Log on to bu 2
				Expect.Call(_jobHelper.SetBusinessUnit(_bu2)).Return(true);
				_jobHelper.LogOffTeleoptiCccDomain();
				Expect.Call(_jobParameters.StateHolder).PropertyBehavior();
			}
			using (_mocks.Playback())
			{
				// Run job 2 and inject the result of job 1
				_jobResult2 = _jobBase2.Run(_bu2, _jobStepsNotToRun, new List<IJobResult> { _jobResult1 }, false, true);
			}

			IJobStepResult resultSuccess = _jobResult2.JobStepResultCollection[0];
			IJobStepResult resultError = _jobResult2.JobStepResultCollection[1];

			Assert.IsFalse(resultSuccess.HasError);
			Assert.AreEqual("Done", resultSuccess.Status);
			Assert.AreEqual("", resultSuccess.BusinessUnitStatus);

			Assert.IsTrue(resultError.HasError);
			Assert.AreEqual("Done", resultError.Status);
			string buStatus = string.Format(CultureInfo.CurrentCulture, "Error in business units: '{0}'", _bu1.Name);
			Assert.AreEqual(buStatus, resultError.BusinessUnitStatus);
		}

		[Test]
		public void VerifyCanGetJobParametersFromJobStep()
		{
			IJobParameters jobParameters = _jobBase1.StepList[0].JobParameters;
			Assert.IsNotNull(jobParameters);
		}

		[Test]
		public void ShouldReturnNullResultWhenLogOntoRaptorFails()
		{
			IBusinessUnit bu = BusinessUnitFactory.CreateSimpleBusinessUnit("myBu");
			var jobParameters = _mocks.StrictMock<IJobParameters>();
			var jobHelper = _mocks.StrictMock<IJobHelper>();
			var job = new JobBase(jobParameters, new List<IJobStep>(), "myJob", false, false);

			using (_mocks.Record())
			{
				Expect.Call(job.JobParameters.Helper).Return(jobHelper);
				Expect.Call(jobHelper.SetBusinessUnit(bu)).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsNull(job.Run(bu, null, null, true, true));
			}
		}
	}

	internal class TestJobStep : JobStepBase
	{
		internal TestJobStep(IJobParameters jobParameters, string name, bool isBusinessUnitIndependent)
			: base(jobParameters)
		{
			Name = name;
			IsBusinessUnitIndependent = isBusinessUnitIndependent;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return 9;
		}
	}
	
	internal class TestErrorJobStep : JobStepBase
	{
		internal TestErrorJobStep(IJobParameters jobParameters, string name)
			: base(jobParameters)
		{
			Name = name;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			throw new ArgumentException("Expected error in test.");
		}
	}
}