using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.ReadModel;
using RowsUpdatedEventArgs = Teleopti.Analytics.Etl.Transformer.ScheduleThreading.RowsUpdatedEventArgs;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
	public class IntradayStageScheduleJobStepTest
    {
        private MockRepository _mock;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
        }

        [Test]
        public void Verify()
        {
            var raptorRepository = _mock.StrictMock<IRaptorRepository>();
            var scenario = _mock.DynamicMock<IScenario>();
            var bu = _mock.DynamicMock<IBusinessUnit>();
            var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
	        jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var scheduleTransformer = _mock.StrictMock<IScheduleTransformer>();
			var stageScheduleJobStep = new IntradayStageScheduleJobStep(jobParameters, scheduleTransformer);
	        var dic = new Dictionary<DateTimePeriod, IScheduleDictionary>();
	        var model = new LastChangedReadModel {LastTime = new DateTime(), ThisTime = new DateTime()};
			Expect.Call(raptorRepository.TruncateSchedule);
			Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> {scenario});
			Expect.Call(scenario.DefaultScenario).Return(true);
			Expect.Call(raptorRepository.LastChangedDate(null, "")).IgnoreArguments().Return(model);
			Expect.Call(() => commonStateHolder.SetThisTime(model, null)).IgnoreArguments();
			Expect.Call(raptorRepository.ChangedDataOnStep(new DateTime(), null, ""))
				.IgnoreArguments()
				.Return(new List<IScheduleChangedReadModel>
					{
						new ScheduleChangedReadModel {Date = DateTime.Today, Person = Guid.NewGuid()}
					});
	        Expect.Call(scenario.Id).Return(Guid.NewGuid());
	        Expect.Call(bu.Id).Return(Guid.NewGuid());
	        Expect.Call(raptorRepository.PersistScheduleChanged(stageScheduleJobStep.BulkInsertDataTable1)).Return(1);
			Expect.Call(commonStateHolder.GetSchedules(new List<IScheduleChangedReadModel>(), scenario)).IgnoreArguments()
                .Return(dic);
	        Expect.Call(commonStateHolder.GetSchedulePartPerPersonAndDate(dic)).Return(new List<IScheduleDay>());
	        
			Expect.Call(
		        ()  =>scheduleTransformer.Transform(new List<IScheduleDay>(), new DateTime(), jobParameters, new ThreadPool())).IgnoreArguments();

			scheduleTransformer.Raise(x => x.RowsUpdatedEvent += null, scheduleTransformer, new RowsUpdatedEventArgs(5));
            _mock.ReplayAll();
            
            stageScheduleJobStep.Run(new List<IJobStep>(), bu, null, false);
			_mock.VerifyAll();
        }


	    [Test]
		public void ShouldOnlyRunDefaultScenario()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
			var scenario = _mock.DynamicMock<IScenario>();
			var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var stageScheduleJobStep = new IntradayStageScheduleJobStep(jobParameters);
			
			Expect.Call(raptorRepository.TruncateSchedule);
			Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			Expect.Call(scenario.DefaultScenario).Return(false);
			
			_mock.ReplayAll();

			stageScheduleJobStep.Run(new List<IJobStep>(), null, null, false);
			_mock.VerifyAll();
		}

		[Test]
		public void ShouldNotRunIfNoChange()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
			var scenario = _mock.DynamicMock<IScenario>();
			var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var stageScheduleJobStep = new IntradayStageScheduleJobStep(jobParameters);
			var model = new LastChangedReadModel {LastTime = new DateTime(), ThisTime = new DateTime()};
			Expect.Call(raptorRepository.TruncateSchedule);
			Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			Expect.Call(scenario.DefaultScenario).Return(true);
			Expect.Call(raptorRepository.LastChangedDate(null, "Schedules")).IgnoreArguments().Return(model);
			Expect.Call(() => commonStateHolder.SetThisTime(model, null)).IgnoreArguments();
			Expect.Call(raptorRepository.ChangedDataOnStep(new DateTime(), null, ""))
				.IgnoreArguments()
				.Return(new List<IScheduleChangedReadModel>());

			_mock.ReplayAll();

			stageScheduleJobStep.Run(new List<IJobStep>(), null, null, false);
			_mock.VerifyAll();
		}

		[Test]
		public void ShouldRunDifferentInFactOnIntraday()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
			var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();

			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var step = new FactScheduleJobStep(jobParameters, true);

			var bu = _mock.DynamicMock<IBusinessUnit>();
			Expect.Call(raptorRepository.FillIntradayScheduleDataMart(bu)).Return(5);
			_mock.ReplayAll();
			step.Run(new List<IJobStep>(), bu, null, true);
		}
    }
}
