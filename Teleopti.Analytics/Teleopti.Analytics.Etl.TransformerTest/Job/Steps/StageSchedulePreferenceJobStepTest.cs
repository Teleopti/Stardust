using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class StageSchedulePreferenceJobStepTest
    {
        private StageSchedulePreferenceJobStep _target;
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
            _jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), null,null);
            _target = new StageSchedulePreferenceJobStep(_jobParameters);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(JobCategoryType.Schedule, _target.JobCategory);
            Assert.AreEqual("stg_schedule_preference",_target.Name);
            Assert.IsFalse(_target.IsBusinessUnitIndependent);
        }
    }
}
