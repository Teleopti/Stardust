using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
    [TestFixture]
    public class CleanupJobTest
    {
        private IJobParameters _parameters;

        [SetUp]
        public void Setup()
        {
            _parameters = JobParametersFactory.SimpleParameters(false);
        }

        [Test]
        public void TestPopulateLoadSteps()
        {
            IList<IJobStep> jobStepList = new CleanupJobCollection(_parameters);

            Assert.IsTrue(jobStepList.Count > 0);
        }
    }
}