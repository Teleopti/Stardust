using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class ScheduleJobStepsTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
            _jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), null,null);
        }

        [Test]
        public void VerifyScheduleJobSteps()
        {
            IList<IJobStep> jobStepList = new ScheduleJobCollection(_jobParameters);

        	foreach (var jobStep in jobStepList)
        	{
        		if (jobStep is StageScheduleJobStep) //Threaded stuff not tested here...
        			continue;
        		jobStep.Run(new List<IJobStep>(), null, null, false);
        	}
        }
    }
}