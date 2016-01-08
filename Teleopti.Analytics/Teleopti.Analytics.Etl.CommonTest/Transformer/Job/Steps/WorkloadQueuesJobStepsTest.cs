using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
    [TestFixture]
    public class WorkloadQueuesJobStepsTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null);
        }

        [Test]
        public void BridgeQueueWorkloadJobStep()
        {
            BridgeQueueWorkloadJobStep ss = new BridgeQueueWorkloadJobStep(_jobParameters);
            IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
            Assert.IsNotNull(jobStepResult);
        }
    }
}
