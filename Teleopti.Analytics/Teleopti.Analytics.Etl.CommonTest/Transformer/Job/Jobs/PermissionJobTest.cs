using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
    [TestFixture]
    public class PermissionJobTest
    {
        private IJobParameters _parameters;

        [Test]
        public void TestPopulateLoadSteps()
        {
            _parameters = JobParametersFactory.SimpleParameters(false);
            IList<IJobStep> jobStepList = new PermissionJobCollection(_parameters);
            Assert.AreEqual(12, jobStepList.Count);
        }

        [Test]
        public void TestPopulateLoadStepsIncludingPm()
        {
            _parameters = JobParametersFactory.SimpleParameters(true);
            IList<IJobStep> jobStepList = new PermissionJobCollection(_parameters);
            Assert.AreEqual(13, jobStepList.Count);
        }
    }
}