using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class WorkloadQueuesJobStepsTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);
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
