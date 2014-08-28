using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class RequestJobsTest
	{
		[SetUp]
		public void Setup()
		{
			_jobParameters.Helper = new JobHelper(_raptorRepository, null, null, null);
		}

		private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
		private readonly IRaptorRepository _raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();

		[Test]
		public void ShouldLoadAndTransformRequestFromDomain()
		{
			IList<IPersonRequest> list = new List<IPersonRequest>();

		    var temp = _jobParameters.JobCategoryDates.GetJobMultipleDateItem(JobCategoryType.Schedule);

		    var period = new DateTimePeriod(temp.StartDateUtcFloor,
		                                    temp.EndDateUtcCeiling);
                                                      
			_raptorRepository.Expect(x => x.LoadRequest(period)).Return(list);
			//var transformer = MockRepository.GenerateMock<IEtlTransformer<IPersonRequest>>();
            var transformer = MockRepository.GenerateMock<IPersonRequestTransformer<IPersonRequest>>();

            using (var stgRequest = new StageRequestJobStep(_jobParameters))
            {
                
                stgRequest.Transformer = transformer;
                IJobStepResult jobStepResult = stgRequest.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
                _raptorRepository.AssertWasCalled(x => x.LoadRequest(period));
            }
		    //transformer.AssertWasCalled(y=>y.Transform(list, 96, stgRequest.BulkInsertDataTable1));


		}

		[Test]
		public void ShouldLoadRequestFromStageToMart()
		{
			using(var factRequest = new FactRequestJobStep(_jobParameters))
			{
                IJobStepResult jobStepResult = factRequest.Run(new List<IJobStep>(), null, null, false);
                Assert.IsNotNull(jobStepResult);
                _raptorRepository.AssertWasCalled(x => x.FillFactRequestMart(new DateTimePeriod(), BusinessUnitFactory.CreateSimpleBusinessUnit("Test BU")), options => options.IgnoreArguments());			    
			}
		}

	}
}
