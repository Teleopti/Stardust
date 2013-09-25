using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class AgentStatisticsJobStepsTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
            _jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null,null);
        }

        [Test]
        public void VerifyAgentStatisticsJobSteps()
        {
            IList<IJobStep> jobStepList = new AgentStatisticsJobCollection(_jobParameters);
            foreach (IJobStep jobStep in jobStepList)
            {
                IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
            }
        }
    }
}
