using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Jobs
{
    [TestFixture]
    public class JobListTest
    {
        private IList<IJob> _jobList;

        [SetUp]
        public void Setup()
        {
            _jobList = new JobCollection(JobParametersFactory.SimpleParameters(false));
        }

        [Test]
        public void VerifyGetJobList()
        {
            Assert.Greater(_jobList.Count, 0);
        }
    }
}