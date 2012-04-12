﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.TransformerInfrastructure;


namespace Teleopti.Analytics.Etl.CommonTest
{
	[TestFixture]
	public class RunControllerTest
	{
		private RunController _target;

		[Test]
		public void ShouldNotifyThatAnotherEtlProcessIsRunning()
		{
			var repository = MockRepository.GenerateMock<IRunControllerRepository>();
			IEtlRunningInformation etlRunningInformation1;
			var etlRunningInformation2 = new EtlRunningInformation
			                             	{
			                             		ComputerName = "myPc",
			                             		StartTime = new DateTime(2012, 4, 4, 2, 0, 0),
			                             		JobName = "myJob",
												IsStartedByService = true
			                             	};

			_target = new RunController(repository);
			repository.Stub(x => x.IsAnotherEtlRunningAJob(out etlRunningInformation1)).Return(true).OutRef(etlRunningInformation2);

			_target.CanIRunAJob(out etlRunningInformation1).Should().Be.False();
			etlRunningInformation1.Should().Be.SameInstanceAs(etlRunningInformation2);
		}

		[Test]
		public void ShouldNotifyThatICanRunEtlJob()
		{
			var repository = MockRepository.GenerateMock<IRunControllerRepository>();
			IEtlRunningInformation etlRunningInformation = null;

			_target = new RunController(repository);
			repository.Stub(x => x.IsAnotherEtlRunningAJob(out etlRunningInformation)).Return(false).OutRef(etlRunningInformation);

			_target.CanIRunAJob(out etlRunningInformation).Should().Be.True();
			etlRunningInformation.Should().Be.Null();
		}

		[Test]
		public void ShouldStartEtlLock()
		{
			_target=new RunController(null);
			const string jobName = "Job name 1";
			var etlJobLock = MockRepository.GenerateMock<IEtlJobLock>();
			etlJobLock.Expect(x => x.CreateLock(jobName, true));
			_target.StartEtlJobRunLock(jobName, true, etlJobLock);

			etlJobLock.VerifyAllExpectations();
		}
	}

}
