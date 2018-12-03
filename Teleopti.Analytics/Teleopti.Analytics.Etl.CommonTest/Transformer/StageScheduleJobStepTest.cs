using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;

using ScenarioFactory = Teleopti.Ccc.TestCommon.FakeData.ScenarioFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
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
            _scheduleDictionary = new ScheduleDictionary(scenario, period2, diffSvc, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
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
			jobParameters.Helper = new JobHelperForTest(raptorRepository, null);
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
