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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using ScenarioFactory=Teleopti.Ccc.TestCommon.FakeData.ScenarioFactory;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class StageScheduleJobStepTest
    {
        private IScheduleDictionary _scheduleDictionary;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            var diffSvc = mocks.StrictMock<IDifferenceCollectionService<IPersistableScheduleData>>();
            var period2 = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            _scheduleDictionary = new ScheduleDictionary(scenario, period2, diffSvc);
        }

        [Test]
        public void Verify()
        {
            MockRepository mockRepository = new MockRepository();
            IRaptorRepository raptorRepository = mockRepository.StrictMock<IRaptorRepository>();
            IScenario scenario = mockRepository.StrictMock<IScenario>();
            ICommonStateHolder commonStateHolder = mockRepository.StrictMock<ICommonStateHolder>();

            using (mockRepository.Record())
            {
                Expect.On(raptorRepository)
                    .Call(raptorRepository.LoadSchedule(new DateTimePeriod(), scenario, commonStateHolder)).IgnoreArguments()
                    .Return(_scheduleDictionary)
                    .Repeat.Any();

                Expect.On(raptorRepository)
                    .Call(raptorRepository.LoadPerson(commonStateHolder))
                    .Return(new List<IPerson>())
                    .Repeat.Any();

            }

            IJobParameters jobParameters = JobParametersFactory.SimpleParameters(false);

            jobParameters.Helper = new JobHelper(raptorRepository, null,null);

            IScheduleTransformer scheduleTransformer = mockRepository.StrictMock<IScheduleTransformer>();
            StageScheduleJobStep stageScheduleJobStep = new StageScheduleJobStep(jobParameters, scheduleTransformer);
            stageScheduleJobStep.Run(new List<IJobStep>(), null, null, false);
        }
    }
}
