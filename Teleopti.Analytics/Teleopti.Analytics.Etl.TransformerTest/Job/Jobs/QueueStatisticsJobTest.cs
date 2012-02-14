using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Jobs
{
    [TestFixture]
    public class QueueStatisticsJobTest
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
            IList<IJobStep> jobStepList = new QueueStatisticsJobCollection(_jobParameters);
            Assert.AreEqual(4, jobStepList.Count);
        }
    }
}