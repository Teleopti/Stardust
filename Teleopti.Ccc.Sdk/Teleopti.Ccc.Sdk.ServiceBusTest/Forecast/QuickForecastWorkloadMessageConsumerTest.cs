using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class QuickForecastWorkloadMessageConsumerTest
	{
		private MockRepository _mocks;
		private ISkillDayRepository _skillDayRep;
		private QuickForecastWorkloadMessageConsumer _target;
		private IOutlierRepository _outlierRep;
		private IWorkloadRepository _workloadRep;
		private IScenarioRepository _scenarioRep;
		private IRepositoryFactory _repFactory;
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IJobResultRepository _jobResultRep;
		private IJobResultFeedback _jobResultFeedback;
		private IMessageBroker _messBroker;
		private IWorkloadDayHelper _workloadDayHelper;
		private IUnitOfWork _unitOfWork;
		private Guid _jobId;
		private QuickForecastWorkloadMessage _mess;
		private IStatisticHelper _statisticHelper;
		private DateOnlyPeriod _statPeriod;
		private IForecastClassesCreator _forecastClassesCreator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillDayRep = _mocks.DynamicMock<ISkillDayRepository>();
			_outlierRep = _mocks.DynamicMock<IOutlierRepository>();
			_workloadRep = _mocks.DynamicMock<IWorkloadRepository>();
			_scenarioRep = _mocks.DynamicMock<IScenarioRepository>();
			_repFactory = _mocks.DynamicMock<IRepositoryFactory>();
			_currentunitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_jobResultRep = _mocks.DynamicMock<IJobResultRepository>();
			_jobResultFeedback = _mocks.DynamicMock<IJobResultFeedback>();
			_messBroker = _mocks.DynamicMock<IMessageBroker>();
			_workloadDayHelper = _mocks.DynamicMock<IWorkloadDayHelper>();
			_statisticHelper = _mocks.DynamicMock<IStatisticHelper>();
			_forecastClassesCreator = _mocks.DynamicMock<IForecastClassesCreator>();
			_target = new QuickForecastWorkloadMessageConsumer(_skillDayRep, _outlierRep, _workloadRep, _scenarioRep, _repFactory,
															   _currentunitOfWorkFactory, _jobResultRep, _jobResultFeedback, _messBroker,
															   _workloadDayHelper, _forecastClassesCreator);
			_unitOfWork =  _mocks.DynamicMock<IUnitOfWork>();

			_jobId = Guid.NewGuid();
			_statPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 31);
			_mess = new QuickForecastWorkloadMessage {JobId = _jobId,StatisticPeriod = _statPeriod, SmoothingStyle = 3,UseDayOfMonth = true};
		}

		[Test]
		public void ShouldExitIfWrongJobId()
		{
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_jobResultRep.Get(_jobId)).Return(null);
			_mocks.ReplayAll();
			_target.Consume(_mess);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldExitIfWrongWorkloadId()
		{
			var jobResult = _mocks.DynamicMock<IJobResult>();
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_jobResultRep.Get(_jobId)).Return(jobResult);
			Expect.Call(_workloadRep.Get(Guid.NewGuid())).IgnoreArguments().Return(null);
			_mocks.ReplayAll();
			_target.Consume(_mess);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldForecast()
		{
			var jobResult = _mocks.DynamicMock<IJobResult>();
			var workload = _mocks.DynamicMock<IWorkload>();
			var scenario = _mocks.DynamicMock<IScenario>();
			var skillDayCalc = _mocks.DynamicMock<ISkillDayCalculator>();
			var totalVolume = _mocks.DynamicMock<ITotalVolume>();
			var validatedRep = _mocks.DynamicMock<IValidatedVolumeDayRepository>();
			var workloadDayTemplateCalculator = _mocks.DynamicMock<IWorkloadDayTemplateCalculator>();
			var taskOwner = _mocks.DynamicMock<ITaskOwner>();
			var teskOwners = new List<ITaskOwner> {taskOwner};
			//var taskOwnerPeriod = _mocks.DynamicMock<ITaskOwnerPeriod>();
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(2013,1,1),new List<ITaskOwner>(),TaskOwnerPeriodType.Other  );
			var template = _mocks.DynamicMock<IWorkloadDayTemplate>();

			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_jobResultRep.Get(_jobId)).Return(jobResult);
			Expect.Call(_workloadRep.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			Expect.Call(() =>_jobResultFeedback.SetJobResult(jobResult, _messBroker));
			Expect.Call(_scenarioRep.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			Expect.Call(_forecastClassesCreator.CreateStatisticHelper(_unitOfWork)).Return(_statisticHelper);
			Expect.Call(_statisticHelper.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());
			Expect.Call(_repFactory.CreateValidatedVolumeDayRepository(_unitOfWork)).Return(validatedRep);

			Expect.Call(validatedRep.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			Expect.Call(validatedRep.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>(),
												false)).Return(new List<ITaskOwner>()).IgnoreArguments();

			Expect.Call(_statisticHelper.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, scenario,
			                                                                    new List<IValidatedVolumeDay>()))
				  .Return(teskOwners);
			Expect.Call(_forecastClassesCreator.GetNewTaskOwnerPeriod(teskOwners)).Return(taskOwnerPeriod);
			Expect.Call(_outlierRep.FindByWorkload(workload)).Return(new List<IOutlier>());
			Expect.Call(_skillDayRep.GetAllSkillDays(_statPeriod, new List<ISkillDay>(), null, scenario,false)).IgnoreArguments()
			      .Return(new Collection<ISkillDay>());
			Expect.Call(_forecastClassesCreator.CreateSkillDayCalculator(null, new List<ISkillDay>(), _statPeriod))
			      .IgnoreArguments()
			      .Return(skillDayCalc);
			Expect.Call(_workloadDayHelper.GetWorkloadDaysFromSkillDays(new List<ISkillDay>(), workload)).IgnoreArguments()
			      .Return(new List<IWorkloadDayBase>());

			
			Expect.Call(_forecastClassesCreator.CreateTotalVolume()).Return(totalVolume);
			totalVolume.Create(null,new List<ITaskOwner>() , new List<IVolumeYear>(),
			                               new List<IOutlier>(), 0, 0, false,workload);
			LastCall.IgnoreArguments();

			Expect.Call(_forecastClassesCreator.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep))
			      .Return(workloadDayTemplateCalculator);
			Expect.Call(() => workloadDayTemplateCalculator.LoadWorkloadDayTemplates(new List<DateOnlyPeriod>(), workload))
			      .IgnoreArguments();
			Expect.Call(workload.GetTemplateAt(TemplateTarget.Workload, 1)).IgnoreArguments().Return(template);
			_mocks.ReplayAll();
			_target.Consume(_mess);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotForecastWhenNoStatistic()
		{
			var jobResult = _mocks.DynamicMock<IJobResult>();
			var workload = _mocks.DynamicMock<IWorkload>();
			var scenario = _mocks.DynamicMock<IScenario>();

			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_jobResultRep.Get(_jobId)).Return(jobResult);
			Expect.Call(_workloadRep.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			Expect.Call(() => _jobResultFeedback.SetJobResult(jobResult, _messBroker));
			Expect.Call(_scenarioRep.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			Expect.Call(_forecastClassesCreator.CreateStatisticHelper(_unitOfWork)).Return(_statisticHelper);
			Expect.Call(_statisticHelper.LoadStatisticData(_statPeriod,workload)).Return(new List<IWorkloadDayBase>());

			_mocks.ReplayAll();
			_target.Consume(_mess);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), Test]
		public void ShouldReportError()
		{
			var jobResult = _mocks.DynamicMock<IJobResult>();
			Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_jobResultRep.Get(_jobId)).Return(jobResult);
			Expect.Call(_workloadRep.Get(Guid.NewGuid())).IgnoreArguments().Throw(new ArgumentOutOfRangeException());
			Expect.Call(() => _jobResultFeedback.ReportProgress(0, "Error")).IgnoreArguments();
			_mocks.ReplayAll();
			_target.Consume(_mess);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldUseCreator()
		{
			var creator = new ForecastClassesCreator(_repFactory);
			var statRep = _mocks.DynamicMock<IStatisticRepository>();
			Assert.That(creator.CreateTotalVolume(), Is.Not.Null);
			Assert.That(creator.CreateSkillDayCalculator(null, new List<ISkillDay>(),new DateOnlyPeriod() ), Is.Not.Null);
			Assert.That(creator.CreateWorkloadDayTemplateCalculator(_statisticHelper,_outlierRep),Is.Not.Null);
			Assert.That(creator.GetNewTaskOwnerPeriod(new List<ITaskOwner>()),Is.Not.Null);
			Expect.Call(_repFactory.CreateStatisticRepository()).Return(statRep);
			_mocks.ReplayAll();
			Assert.That(creator.CreateStatisticHelper(_unitOfWork),Is.Not.Null);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSendMessageForEachWorkload()
		{
			var bus = _mocks.DynamicMock<IServiceBus>();
			var consumer = new QuickForecastWorkloadsMessageConsumer(bus);
			var mess = new QuickForecastWorkloadsMessage {WorkloadIds = new Collection<Guid> {Guid.NewGuid(), Guid.NewGuid()}};
			Expect.Call(() => bus.Send(new QuickForecastWorkloadMessage())).IgnoreArguments().Repeat.Twice();
			_mocks.ReplayAll();
			consumer.Consume(mess);
			_mocks.VerifyAll();
		}
	}

	

	
}