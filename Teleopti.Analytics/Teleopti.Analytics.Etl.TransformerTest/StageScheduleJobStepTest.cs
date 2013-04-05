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
            var mocks = new MockRepository();
            var diffSvc = mocks.StrictMock<IDifferenceCollectionService<IPersistableScheduleData>>();
            var period2 = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            _scheduleDictionary = new ScheduleDictionary(scenario, period2, diffSvc);
			_mock = new MockRepository();
        }

        [Test]
        public void Verify()
        {
            
            var raptorRepository = _mock.StrictMock<IRaptorRepository>();
            var scenario = _mock.StrictMock<IScenario>();
            var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			var scheduleTransformer = _mock.DynamicMock<IScheduleTransformer>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelper(raptorRepository, null, null);
			var stageScheduleJobStep = new StageScheduleJobStep(jobParameters, scheduleTransformer);

	        Expect.Call(() => raptorRepository.TruncateSchedule());
	        Expect.Call(raptorRepository.LoadScenario()).Return(new List<IScenario>{scenario});
            Expect.Call(raptorRepository.LoadSchedule(new DateTimePeriod(), scenario, commonStateHolder)).IgnoreArguments()
                .Return(_scheduleDictionary)
                .Repeat.Any();

            Expect.Call(raptorRepository.LoadPerson(commonStateHolder))
                .Return(new List<IPerson>())
                .Repeat.Any();

            _mock.ReplayAll();
            
            stageScheduleJobStep.Run(new List<IJobStep>(), null, null, false);
			_mock.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckIfNeedRun()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
            var currentBusinessUnit = _mock.DynamicMock<IBusinessUnit>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelper(raptorRepository, null, null);
			var needRunChecker = _mock.DynamicMock<INeedToRunChecker>();
			var stageScheduleJobStep = new StageScheduleJobStep(jobParameters, needRunChecker);

			Expect.Call(raptorRepository.TruncateSchedule);
			Expect.Call(needRunChecker.NeedToRun(new DateTimePeriod(), raptorRepository, currentBusinessUnit,""))
				.Return(false)
				.IgnoreArguments();
			_mock.ReplayAll();
			stageScheduleJobStep.Run(new List<IJobStep>(), currentBusinessUnit, null, false);
			_mock.VerifyAll();
		}
    }
}
