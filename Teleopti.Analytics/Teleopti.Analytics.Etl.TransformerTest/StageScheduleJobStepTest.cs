using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using ScenarioFactory=Teleopti.Ccc.TestCommon.FakeData.ScenarioFactory;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class StageScheduleJobStepTest
    {
        private IScheduleDictionary _scheduleDictionary;
	    private MockRepository _mock;

	    [SetUp]
        public void Setup()
        {
			_mock = new MockRepository();
			var diffSvc = _mock.StrictMock<IDifferenceCollectionService<IPersistableScheduleData>>();
            var period2 = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            _scheduleDictionary = new ScheduleDictionary(scenario, period2, diffSvc);
        }

        [Test]
        public void Verify()
        {
            var raptorRepository = _mock.StrictMock<IRaptorRepository>();
            var scenario = _mock.StrictMock<IScenario>();
            var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			var scheduleTransformer = _mock.DynamicMock<IScheduleTransformer>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var stageScheduleJobStep = new StageScheduleJobStep(jobParameters, scheduleTransformer);

	        Expect.Call(raptorRepository.TruncateSchedule);
	        Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			Expect.Call(commonStateHolder.GetSchedules(new DateTimePeriod(), scenario)).IgnoreArguments()
                .Return(_scheduleDictionary);
	        Expect.Call(commonStateHolder.GetSchedulePartPerPersonAndDate(_scheduleDictionary))
				.Return(new List<IScheduleDay>());

            _mock.ReplayAll();
            
            stageScheduleJobStep.Run(new List<IJobStep>(), null, null, false);
			_mock.VerifyAll();
        }
    }
}
