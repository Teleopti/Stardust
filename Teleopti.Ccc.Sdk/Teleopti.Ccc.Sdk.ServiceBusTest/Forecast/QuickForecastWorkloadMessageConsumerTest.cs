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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class QuickForecastWorkloadMessageConsumerTest
	{
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
		private IMessageBrokerComposite _messBroker;
		private IWorkloadDayHelper _workloadDayHelper;
		private IUnitOfWork _unitOfWork;
		private Guid _jobId;
		private QuickForecastWorkloadMessage _mess;
		private IStatisticHelper _statisticHelper;
		private DateOnlyPeriod _statPeriod;
		private IForecastClassesCreator _forecastClassesCreator;
		private IMultisiteDayRepository _multisiteDayRep;
		private IRepository<IMultisiteSkill> _skillRep;

		[SetUp]
		public void Setup()
		{
			_skillDayRep = MockRepository.GenerateMock<ISkillDayRepository>();
			_multisiteDayRep = MockRepository.GenerateMock<IMultisiteDayRepository>();
			_outlierRep = MockRepository.GenerateMock<IOutlierRepository>();
			_workloadRep = MockRepository.GenerateMock<IWorkloadRepository>();
			_skillRep = MockRepository.GenerateMock<IRepository<IMultisiteSkill>>();
			_scenarioRep = MockRepository.GenerateMock<IScenarioRepository>();
			_repFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_currentunitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_jobResultRep = MockRepository.GenerateMock<IJobResultRepository>();
			_jobResultFeedback = MockRepository.GenerateMock<IJobResultFeedback>();
			_messBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_workloadDayHelper = MockRepository.GenerateMock<IWorkloadDayHelper>();
			_statisticHelper = MockRepository.GenerateMock<IStatisticHelper>();
			_forecastClassesCreator = MockRepository.GenerateMock<IForecastClassesCreator>();
			_target = new QuickForecastWorkloadMessageConsumer(_skillDayRep, _multisiteDayRep, _outlierRep, _workloadRep, _skillRep, _scenarioRep, _repFactory,
															   _currentunitOfWorkFactory, _jobResultRep, _jobResultFeedback, _messBroker,
															   _workloadDayHelper, _forecastClassesCreator);
			_unitOfWork =  MockRepository.GenerateMock<IUnitOfWork>();

			_jobId = Guid.NewGuid();
			_statPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 31);
			_mess = new QuickForecastWorkloadMessage {JobId = _jobId,StatisticPeriod = _statPeriod, SmoothingStyle = 3,UseDayOfMonth = true};
		}

		[Test]
		public void ShouldExitIfWrongJobId()
		{
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(null);
			
			_target.Consume(_mess);
			
			_workloadRep.AssertWasNotCalled(x => x.Get(Guid.Empty),o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldExitIfWrongWorkloadId()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(null);
			
			_target.Consume(_mess);

			_scenarioRep.AssertWasNotCalled(x => x.Get(Guid.Empty), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldForecast()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("Sales"));
			var scenario = MockRepository.GenerateMock<IScenario>();
			var skillDayCalc = MockRepository.GenerateMock<ISkillDayCalculator>();
			var totalVolume = MockRepository.GenerateMock<ITotalVolume>();
			var validatedRep = MockRepository.GenerateMock<IValidatedVolumeDayRepository>();
			var workloadDayTemplateCalculator = MockRepository.GenerateMock<IWorkloadDayTemplateCalculator>();
			var taskOwner = MockRepository.GenerateMock<ITaskOwner>();
			var teskOwners = new List<ITaskOwner> {taskOwner};
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(2013,1,1),new List<ITaskOwner>(),TaskOwnerPeriodType.Other  );
			
			_currentunitOfWorkFactory.Stub(x=> x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x=> x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_forecastClassesCreator.Stub(x => x.CreateStatisticHelper(_unitOfWork)).Return(_statisticHelper);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());
			_repFactory.Stub(x => x.CreateValidatedVolumeDayRepository(_unitOfWork)).Return(validatedRep);

			validatedRep.Stub(x=> x.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			validatedRep.Stub(x => x.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>(), false)).Return(new List<ITaskOwner>()).IgnoreArguments();

			_statisticHelper.Stub(x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, scenario, new List<IValidatedVolumeDay>())).Return(teskOwners);
			_forecastClassesCreator.Stub(x => x.GetNewTaskOwnerPeriod(teskOwners)).Return(taskOwnerPeriod);
			_outlierRep.Stub(x => x.FindByWorkload(workload)).Return(new List<IOutlier>());
			_skillDayRep.Stub(x => x.GetAllSkillDays(_statPeriod, new List<ISkillDay>(), null, scenario, _ => { })).IgnoreArguments().Return(new Collection<ISkillDay>());
			_forecastClassesCreator.Stub(x => x.CreateSkillDayCalculator(null, new List<ISkillDay>(), _statPeriod)).IgnoreArguments().Return(skillDayCalc);
			_workloadDayHelper.Stub(x => x.GetWorkloadDaysFromSkillDays(new List<ISkillDay>(), workload)).IgnoreArguments().Return(new List<IWorkloadDayBase>());

			_forecastClassesCreator.Stub(x => x.CreateTotalVolume()).Return(totalVolume);
			totalVolume.Create(null,new List<ITaskOwner>() , new List<IVolumeYear>(), new List<IOutlier>(), 0, 0, false,workload);

			_forecastClassesCreator.Stub(x => x.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep)).Return(workloadDayTemplateCalculator);
			_skillRep.Stub(x => x.Get(Guid.Empty)).Return(null);
			
			_target.Consume(_mess);

			workloadDayTemplateCalculator.AssertWasCalled(x => x.LoadWorkloadDayTemplates(new List<DateOnlyPeriod>(), workload),
			                                              o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldForecastMultisiteSkill()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("Sales"));
			var scenario = MockRepository.GenerateMock<IScenario>();
			var skillDayCalc = MockRepository.GenerateMock<ISkillDayCalculator>();
			var totalVolume = MockRepository.GenerateMock<ITotalVolume>();
			var validatedRep = MockRepository.GenerateMock<IValidatedVolumeDayRepository>();
			var workloadDayTemplateCalculator = MockRepository.GenerateMock<IWorkloadDayTemplateCalculator>();
			var taskOwner = MockRepository.GenerateMock<ITaskOwner>();
			var teskOwners = new List<ITaskOwner> { taskOwner };
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(2013, 1, 1), new List<ITaskOwner>(), TaskOwnerPeriodType.Other);
			
			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_forecastClassesCreator.Stub(x => x.CreateStatisticHelper(_unitOfWork)).Return(_statisticHelper);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());
			_repFactory.Stub(x => x.CreateValidatedVolumeDayRepository(_unitOfWork)).Return(validatedRep);

			validatedRep.Stub(x => x.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			validatedRep.Stub(x => x.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>(), false)).Return(new List<ITaskOwner>()).IgnoreArguments();

			_statisticHelper.Stub(x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, scenario, new List<IValidatedVolumeDay>())).Return(teskOwners);
			_forecastClassesCreator.Stub(x => x.GetNewTaskOwnerPeriod(teskOwners)).Return(taskOwnerPeriod);
			_outlierRep.Stub(x => x.FindByWorkload(workload)).Return(new List<IOutlier>());
			_skillDayRep.Stub(x => x.GetAllSkillDays(_statPeriod, new List<ISkillDay>(), null, scenario, _ => { })).IgnoreArguments().Return(new Collection<ISkillDay>());
			_multisiteDayRep.Stub(x => x.GetAllMultisiteDays(_statPeriod, new List<IMultisiteDay>(), null, scenario, true)).IgnoreArguments().Return(new Collection<IMultisiteDay>());
			_forecastClassesCreator.Stub(x => x.CreateSkillDayCalculator(null, new List<ISkillDay>(), new List<IMultisiteDay>(), new Dictionary<IChildSkill, ICollection<ISkillDay>>(), _statPeriod)).IgnoreArguments().Return(skillDayCalc);
			_workloadDayHelper.Stub(x => x.GetWorkloadDaysFromSkillDays(new List<ISkillDay>(), workload)).IgnoreArguments().Return(new List<IWorkloadDayBase>());

			_forecastClassesCreator.Stub(x => x.CreateTotalVolume()).Return(totalVolume);
			totalVolume.Create(null, new List<ITaskOwner>(), new List<IVolumeYear>(), new List<IOutlier>(), 0, 0, false, workload);

			_forecastClassesCreator.Stub(x => x.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep)).Return(workloadDayTemplateCalculator);
			_skillRep.Stub(x => x.Get(Guid.Empty)).Return(SkillFactory.CreateMultisiteSkill("Multi sales"));

			_target.Consume(_mess);

			workloadDayTemplateCalculator.AssertWasCalled(x => x.LoadWorkloadDayTemplates(new List<DateOnlyPeriod>(), workload),
														  o => o.IgnoreArguments());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotForecastWhenNoStatistic()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			var scenario = MockRepository.GenerateMock<IScenario>();

			_currentunitOfWorkFactory.Stub(x=> x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x=> x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x=> x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x=> x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x=> x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_forecastClassesCreator.Stub(x=> x.CreateStatisticHelper(_unitOfWork)).Return(_statisticHelper);
			_statisticHelper.Stub(x=> x.LoadStatisticData(_statPeriod,workload)).Return(new List<IWorkloadDayBase>());

			_target.Consume(_mess);

			_statisticHelper.AssertWasNotCalled(
				x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, scenario, new List<IValidatedVolumeDay>()),
				o => o.IgnoreArguments());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), Test]
		public void ShouldReportError()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();

			_currentunitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x=> x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x=> x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x=> x.Get(Guid.NewGuid())).IgnoreArguments().Throw(new ArgumentOutOfRangeException());
			
			_target.Consume(_mess);

			_jobResultFeedback.AssertWasCalled(x=>x.ReportProgress(0, "Error"),o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldUseCreator()
		{
			var creator = new ForecastClassesCreator(_repFactory);
			var statRep = MockRepository.GenerateMock<IStatisticRepository>();
			Assert.That(creator.CreateTotalVolume(), Is.Not.Null);
			Assert.That(creator.CreateSkillDayCalculator(null, new List<ISkillDay>(),new DateOnlyPeriod() ), Is.Not.Null);
			Assert.That(creator.CreateSkillDayCalculator(SkillFactory.CreateMultisiteSkill("Phone"), new List<ISkillDay>(),new List<IMultisiteDay>(), new Dictionary<IChildSkill, ICollection<ISkillDay>>(), new DateOnlyPeriod() ), Is.Not.Null);
			Assert.That(creator.CreateWorkloadDayTemplateCalculator(_statisticHelper,_outlierRep),Is.Not.Null);
			Assert.That(creator.GetNewTaskOwnerPeriod(new List<ITaskOwner>()),Is.Not.Null);

			_repFactory.Stub(x => x.CreateStatisticRepository()).Return(statRep);		
			Assert.That(creator.CreateStatisticHelper(_unitOfWork),Is.Not.Null);
		}

		[Test]
		public void ShouldSendMessageForEachWorkload()
		{
			var bus = MockRepository.GenerateMock<IServiceBus>();
			var consumer = new QuickForecastWorkloadsMessageConsumer(bus);
			var mess = new QuickForecastWorkloadsMessage {WorkloadIds = new Collection<Guid> {Guid.NewGuid(), Guid.NewGuid()}};
			
			consumer.Consume(mess);

			bus.AssertWasCalled(x => x.Send(new QuickForecastWorkloadMessage()), o => o.IgnoreArguments().Repeat.Twice());
		}
	}
}