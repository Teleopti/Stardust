using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;

namespace Teleopti.Analytics.Etl.TransformerTest.Job
{
    [TestFixture]
    public class CustomEventArgsTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            IJobResult jobResult = new JobResult(null, null);
            IJobStepResult jobStepResult = new JobStepResult("Test", 0, 0, null, null);

            jobResult.Name = "Test1";
            //jobStepResult.Name = "Test2";

            _customEventArgs1 = new CustomEventArgs(jobResult);
            _customEventArgs2 = new CustomEventArgs(jobStepResult);
        }

        #endregion

        private CustomEventArgs _customEventArgs1;
        private CustomEventArgs _customEventArgs2;


        [Test]
        public void CustomEventArgs()
        {

            IJobResult jobResult = _customEventArgs1.JobResult;
            IJobStepResult jobStepResult = _customEventArgs2.JobStepResult;

            Assert.IsNotNull(jobResult);
            Assert.IsNotNull(jobStepResult);

        }
    }
}