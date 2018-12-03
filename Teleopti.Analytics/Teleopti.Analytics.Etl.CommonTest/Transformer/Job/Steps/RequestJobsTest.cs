using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class RequestJobsTest
	{
		[Test]
		public void ShouldLoadAndTransformRequestFromDomain()
		{
			IJobParameters jobParameters = JobParametersFactory.SimpleParameters(false);
			IRaptorRepository raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();
			jobParameters.Helper = new JobHelperForTest(raptorRepository, null);

			IList<IPersonRequest> list = new List<IPersonRequest>();

		    var temp = jobParameters.JobCategoryDates.GetJobMultipleDateItem(JobCategoryType.Schedule);

		    var period = new DateTimePeriod(temp.StartDateUtcFloor,
		                                    temp.EndDateUtcCeiling);
                                                      
			raptorRepository.Expect(x => x.LoadRequest(period)).Return(list);
            var transformer = MockRepository.GenerateMock<IPersonRequestTransformer<IPersonRequest>>();

            using (var stgRequest = new StageRequestJobStep(jobParameters))
            {
                
                stgRequest.Transformer = transformer;
                IJobStepResult jobStepResult = stgRequest.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
                raptorRepository.AssertWasCalled(x => x.LoadRequest(period));
            }
		}

		[Test]
		public void ShouldLoadRequestFromStageToMart()
		{
			IJobParameters jobParameters = JobParametersFactory.SimpleParameters(false);
			IRaptorRepository raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();
			jobParameters.Helper = new JobHelperForTest(raptorRepository, null);

			using (var factRequest = new FactRequestJobStep(jobParameters))
			{
                IJobStepResult jobStepResult = factRequest.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
                raptorRepository.AssertWasCalled(x => x.FillFactRequestMart(new DateTimePeriod(), BusinessUnitFactory.CreateSimpleBusinessUnit("Test BU")), options => options.IgnoreArguments());			    
			}
		}

	}
}
