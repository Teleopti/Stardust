using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Jobs
{
	[TestFixture]
	public class RequestJobTest
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
	        IList<IJobStep> jobStepList = new List<IJobStep>
		        {
			        new StageRequestJobStep(_jobParameters),
			        new FactRequestJobStep(_jobParameters),
			        new FactRequestedDaysJobStep(_jobParameters)
		        };
            Assert.AreEqual(3, jobStepList.Count);
        }
	}
}



