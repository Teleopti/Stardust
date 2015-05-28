using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
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