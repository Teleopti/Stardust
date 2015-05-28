using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
    [TestFixture]
    public class WorkloadQueuesJobTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
        }

        [Test]
        public void TestPopulateLoadSteps()
        {
            IList<IJobStep> jobStepList = new WorkloadQueuesJobCollection(_jobParameters);
            Assert.AreEqual(8, jobStepList.Count);
        }
    }
}