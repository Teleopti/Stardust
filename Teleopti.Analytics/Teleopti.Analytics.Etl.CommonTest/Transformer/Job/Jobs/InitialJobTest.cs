using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
    [TestFixture]
    public class InitialJobTest
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
            IList<IJobStep> jobStepList = new InitialJobCollection(_jobParameters);
            Assert.AreEqual(7, jobStepList.Count);
        }
    }
}