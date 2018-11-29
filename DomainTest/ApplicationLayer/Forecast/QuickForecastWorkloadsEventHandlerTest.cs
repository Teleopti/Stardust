using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class QuickForecastWorkloadEventHandlerTest
	{
		private ISkillDayRepository _skillDayRep;
		private QuickForecastWorkloadsEventHandlerHangfire _target;
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
		private ICollection<Guid> _workloadIds;
		private QuickForecastWorkloadsEvent _mess;
		private IStatisticHelper _statisticHelper;
		private DateOnlyPeriod _statPeriod;
		private IForecastClassesCreator _forecastClassesCreator;
		private IMultisiteDayRepository _multisiteDayRep;
		private IRepository<IMultisiteSkill> _skillRep;
		private IValidatedVolumeDayRepository _validatedVolumeDayRepo;
		private LegacyFakeEventPublisher _eventPublisher;
		private ICurrentUnitOfWork _currentUnitOfWork;

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
			_statisticHelper = MockRepository.GenerateMock<IStatisticHelper>();
			_validatedVolumeDayRepo = MockRepository.GenerateMock<IValidatedVolumeDayRepository>();
			_eventPublisher = new LegacyFakeEventPublisher();
			_currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();

			_target = new QuickForecastWorkloadsEventHandlerHangfire(_workloadRep, _multisiteDayRep, _outlierRep, _skillDayRep, _scenarioRep, _jobResultRep, _jobResultFeedback,
				_workloadDayHelper, _forecastClassesCreator, _statisticHelper, _validatedVolumeDayRepo, _messBroker, _skillRep, _eventPublisher, _currentUnitOfWork);

			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

			_jobId = Guid.NewGuid();
			_workloadIds = new Collection<Guid> { Guid.NewGuid() };

			_statPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 31);

			_mess = new QuickForecastWorkloadsEvent
			{
				JobId = _jobId,
				SmoothingStyle = 3,
				WorkloadIds = _workloadIds,
				UseDayOfMonth = true,
				StatisticPeriodStart = _statPeriod.StartDate.Date,
				StatisticPeriodEnd = _statPeriod.EndDate.Date
			};
		}

		[Test]
		public void ShouldExitIfWrongJobId()
		{
			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(null);

			_target.Handle(_mess);

			_workloadRep.AssertWasNotCalled(x => x.Get(Guid.Empty), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldExitIfWrongWorkloadId()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(null);

			_target.Handle(_mess);

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
			var teskOwners = new List<ITaskOwner> { taskOwner };
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(2013, 1, 1), new List<ITaskOwner>(), TaskOwnerPeriodType.Other);

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());

			validatedRep.Stub(x => x.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			validatedRep.Stub(x => x.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>())).Return(new List<ITaskOwner>()).IgnoreArguments();

			_statisticHelper.Stub(x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, new List<IValidatedVolumeDay>())).Return(teskOwners);
			_forecastClassesCreator.Stub(x => x.GetNewTaskOwnerPeriod(teskOwners)).Return(taskOwnerPeriod);
			_outlierRep.Stub(x => x.FindByWorkload(workload)).Return(new List<IOutlier>());
			_skillDayRep.Stub(x => x.GetAllSkillDays(_statPeriod, new List<ISkillDay>(), null, scenario, _ => { })).IgnoreArguments().Return(new Collection<ISkillDay>());
			_forecastClassesCreator.Stub(x => x.CreateSkillDayCalculator(null, new List<ISkillDay>(), _statPeriod)).IgnoreArguments().Return(skillDayCalc);
			_workloadDayHelper.Stub(x => x.GetWorkloadDaysFromSkillDays(new List<ISkillDay>(), workload)).IgnoreArguments().Return(new List<IWorkloadDayBase>());

			_forecastClassesCreator.Stub(x => x.CreateTotalVolume()).Return(totalVolume);
			totalVolume.Create(null, new List<ITaskOwner>(), new List<IVolumeYear>(), new List<IOutlier>(), 0, 0, false, workload);

			_forecastClassesCreator.Stub(x => x.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep)).Return(workloadDayTemplateCalculator);
			_skillRep.Stub(x => x.Get(Guid.Empty)).Return(null);

			_target.Handle(_mess);

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

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());

			validatedRep.Stub(x => x.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			validatedRep.Stub(x => x.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>())).Return(new List<ITaskOwner>()).IgnoreArguments();

			_statisticHelper.Stub(x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, new List<IValidatedVolumeDay>())).Return(teskOwners);
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

			_target.Handle(_mess);

			workloadDayTemplateCalculator.AssertWasCalled(x => x.LoadWorkloadDayTemplates(new List<DateOnlyPeriod>(), workload),
														  o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldForecastAndPublishNextEvent()
		{
			_workloadIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
			_mess.WorkloadIds = _workloadIds;

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


			_currentUnitOfWork.Stub(a => a.Current()).Return(new FakeUnitOfWork());

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());

			validatedRep.Stub(x => x.FindRange(_statPeriod, workload)).Return(new Collection<IValidatedVolumeDay>());
			validatedRep.Stub(x => x.MatchDays(workload, new BindingList<ITaskOwner>(), new Collection<IValidatedVolumeDay>())).Return(new List<ITaskOwner>()).IgnoreArguments();

			_statisticHelper.Stub(x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, new List<IValidatedVolumeDay>())).Return(teskOwners);
			_forecastClassesCreator.Stub(x => x.GetNewTaskOwnerPeriod(teskOwners)).Return(taskOwnerPeriod);
			_outlierRep.Stub(x => x.FindByWorkload(workload)).Return(new List<IOutlier>());
			_skillDayRep.Stub(x => x.GetAllSkillDays(_statPeriod, new List<ISkillDay>(), null, scenario, _ => { })).IgnoreArguments().Return(new Collection<ISkillDay>());
			_forecastClassesCreator.Stub(x => x.CreateSkillDayCalculator(null, new List<ISkillDay>(), _statPeriod)).IgnoreArguments().Return(skillDayCalc);
			_workloadDayHelper.Stub(x => x.GetWorkloadDaysFromSkillDays(new List<ISkillDay>(), workload)).IgnoreArguments().Return(new List<IWorkloadDayBase>());

			_forecastClassesCreator.Stub(x => x.CreateTotalVolume()).Return(totalVolume);
			totalVolume.Create(null, new List<ITaskOwner>(), new List<IVolumeYear>(), new List<IOutlier>(), 0, 0, false, workload);

			_forecastClassesCreator.Stub(x => x.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep)).Return(workloadDayTemplateCalculator);
			_skillRep.Stub(x => x.Get(Guid.Empty)).Return(null);

			_target.Handle(_mess);

			workloadDayTemplateCalculator.AssertWasCalled(x => x.LoadWorkloadDayTemplates(new List<DateOnlyPeriod>(), workload),
														  o => o.IgnoreArguments());

			_eventPublisher.PublishedEvents.Should().Not.Be.Empty();
			var publishedEvent = _eventPublisher.PublishedEvents.First() as QuickForecastWorkloadsEvent;
			publishedEvent.Should().Not.Be.Null();
			publishedEvent.CurrentWorkloadId.Should().Be.EqualTo(_workloadIds.Last());

		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotForecastWhenNoStatistic()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();
			var workload = MockRepository.GenerateMock<IWorkload>();
			var scenario = MockRepository.GenerateMock<IScenario>();

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(workload);
			_scenarioRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(scenario);
			_statisticHelper.Stub(x => x.LoadStatisticData(_statPeriod, workload)).Return(new List<IWorkloadDayBase>());

			var target = new QuickForecastWorkloadsEventHandlerHangfire(_workloadRep, _multisiteDayRep, _outlierRep, _skillDayRep, _scenarioRep, _jobResultRep, _jobResultFeedback,
				_workloadDayHelper, _forecastClassesCreator, _statisticHelper, null, _messBroker, _skillRep, _eventPublisher, _currentUnitOfWork);

			target.Handle(_mess);

			_statisticHelper.AssertWasNotCalled(
				x => x.GetWorkloadDaysWithValidatedStatistics(_statPeriod, workload, new List<IValidatedVolumeDay>()),
				o => o.IgnoreArguments());
		}

		[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), Test]
		public void ShouldReportError()
		{
			var jobResult = MockRepository.GenerateMock<IJobResult>();

			_currentunitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_jobResultRep.Stub(x => x.Get(_jobId)).Return(jobResult);
			_workloadRep.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Throw(new ArgumentOutOfRangeException());

			_target.Handle(_mess);

			_jobResultFeedback.AssertWasCalled(x => x.ReportProgress(0, "Error"), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldUseCreator()
		{
			var creator = new ForecastClassesCreator();
			var statRep = MockRepository.GenerateMock<IStatisticRepository>();
			Assert.That(creator.CreateTotalVolume(), Is.Not.Null);
			Assert.That(creator.CreateSkillDayCalculator(null, new List<ISkillDay>(), new DateOnlyPeriod()), Is.Not.Null);
			Assert.That(creator.CreateSkillDayCalculator(SkillFactory.CreateMultisiteSkill("Phone"), new List<ISkillDay>(), new List<IMultisiteDay>(), new Dictionary<IChildSkill, ICollection<ISkillDay>>(), new DateOnlyPeriod()), Is.Not.Null);
			Assert.That(creator.CreateWorkloadDayTemplateCalculator(_statisticHelper, _outlierRep), Is.Not.Null);
			Assert.That(creator.GetNewTaskOwnerPeriod(new List<ITaskOwner>()), Is.Not.Null);

			_repFactory.Stub(x => x.CreateStatisticRepository()).Return(statRep);
		}
	}

}