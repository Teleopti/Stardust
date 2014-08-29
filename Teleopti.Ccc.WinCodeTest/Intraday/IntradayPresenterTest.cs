using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class IntradayPresenterTest : IDisposable
    {
        private IntradayPresenter _target;
        private readonly DateOnlyPeriod _period = new DateOnlyPeriod(new DateOnly(2013, 1, 1), new DateOnly(2013, 1, 2));
        private readonly DateOnlyPeriod _periodNow = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));
        private IList<IPerson> _persons;
        private IIntradayView _view;
        private IMessageBrokerComposite _messageBroker;
        private IRtaStateHolder _rtaStateHolder;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISchedulingResultLoader _schedulingResultLoader;
        private ISchedulerStateHolder _schedulerStateHolder;
		private IStatisticRepository _statisticRepository;
        private IRepositoryFactory _repositoryFactory;
        private IScenario _scenario;
        private IEventAggregator _eventAggregator;
				private IScheduleDifferenceSaver _scheduleDictionarySaver;
        private OnEventScheduleMessageCommand _scheduleCommand;
        private OnEventForecastDataMessageCommand _forecastCommand;
        private OnEventStatisticMessageCommand _statisticCommand;
        private LoadStatisticsAndActualHeadsCommand _loadStatisticCommand;
        private OnEventMeetingMessageCommand _meetingCommand;
		private IDifferenceCollectionService<IPersistableScheduleData> _differenceService;

				[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
				public void Setup()
				{
					_eventAggregator = new EventAggregator();
					_view = MockRepository.GenerateMock<IIntradayView>();
					_scenario = ScenarioFactory.CreateScenarioAggregate();
					_persons = new List<IPerson> { PersonFactory.CreatePerson() };
					_schedulingResultLoader = MockRepository.GenerateMock<ISchedulingResultLoader>();
					_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
					_rtaStateHolder = MockRepository.GenerateMock<IRtaStateHolder>();
					_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
					_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
					_scheduleDictionarySaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
					_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipal.Current.Regional.TimeZone), _persons);
					_statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
					_differenceService = MockRepository.GenerateMock<IDifferenceCollectionService<IPersistableScheduleData>>();
					_scheduleCommand = MockRepository.GenerateMock<OnEventScheduleMessageCommand>();
					_meetingCommand = MockRepository.GenerateMock<OnEventMeetingMessageCommand>();
					_forecastCommand = MockRepository.GenerateMock<OnEventForecastDataMessageCommand>();
					_statisticCommand = MockRepository.GenerateMock<OnEventStatisticMessageCommand>();
					_loadStatisticCommand = MockRepository.GenerateMock<LoadStatisticsAndActualHeadsCommand>((IStatisticRepository)null);

					_schedulingResultLoader.Stub(x => x.SchedulerState).Return(_schedulerStateHolder);

					_target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker,
																					_rtaStateHolder, _eventAggregator,
																					_scheduleDictionarySaver, _unitOfWorkFactory,
																					_repositoryFactory, _differenceService, _statisticCommand, _forecastCommand,
																					_scheduleCommand, _meetingCommand, _loadStatisticCommand);
				}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyHandlesDateIfTodayIsIncludedInSelection()
        {
            _schedulingResultLoader = MockRepository.GenerateMock<ISchedulingResultLoader>();
            _schedulingResultLoader.Stub(x => x.SchedulerState).Return(new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_periodNow, TeleoptiPrincipal.Current.Regional.TimeZone), _persons)).Repeat.AtLeastOnce();

            _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder, _eventAggregator,
																						_scheduleDictionarySaver, _unitOfWorkFactory, 
                                            _repositoryFactory, _differenceService, _statisticCommand, _forecastCommand,
                                            _scheduleCommand, _meetingCommand, _loadStatisticCommand);

            Assert.AreEqual(DateOnly.Today, _target.IntradayDate);
            Assert.IsFalse(_target.HistoryOnly);
        }

		[Test]
		public void ShouldUnregisterMessageBrokerCallbacks()
		{
			_target.UnregisterMessageBrokerEvents();

			_messageBroker.AssertWasCalled(x => x.UnregisterSubscription(_target.OnEventActualAgentStateMessageHandler));
			_messageBroker.AssertWasCalled(x => x.UnregisterSubscription(_target.OnEventForecastDataMessageHandler));
			_messageBroker.AssertWasCalled(x => x.UnregisterSubscription(_target.OnEventMeetingMessageHandler));
			_messageBroker.AssertWasCalled(x => x.UnregisterSubscription(_target.OnEventScheduleMessageHandler));
			_messageBroker.AssertWasCalled(x => x.UnregisterSubscription(_target.OnEventStatisticMessageHandler));
		}

        [Test]
        public void VerifyMultiplicatorDefinitionSetsCanBeRead()
        {
            var multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            _schedulingResultLoader.Stub(x => x.MultiplicatorDefinitionSets).Return(multiplicatorDefinitionSets);

            Assert.AreEqual(multiplicatorDefinitionSets, _target.MultiplicatorDefinitionSets);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyPrepareSkillIntradayCollection()
        {
            var period = _schedulerStateHolder.RequestedPeriod.Period();
            var smallPeriod = new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15));
            var skill = MockRepository.GenerateMock<ISkill>();
            var skillDay = MockRepository.GenerateMock<ISkillDay>();

            ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod, new Task(),
                                                                       ServiceAgreement.DefaultValues());
            ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod.MovePeriod(TimeSpan.FromMinutes(15)), new Task(),
                                                                       ServiceAgreement.DefaultValues());
            ISkillStaffPeriod skillStaffPeriod3 =
                SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod.MovePeriod(TimeSpan.FromMinutes(15).Add(TimeSpan.FromDays(1))),
                                     new Task(), ServiceAgreement.DefaultValues());

			skillStaffPeriod1.SetSkillDay(skillDay);
			skillStaffPeriod2.SetSkillDay(skillDay);
			skillStaffPeriod3.SetSkillDay(skillDay);

            skill.Stub(x => x.Name).Return("SkillTest");
            skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(
                new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3 }));

            _schedulerStateHolder.SchedulingResultState.Skills.Add(skill);
            _schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
            _schedulerStateHolder.SchedulingResultState.SkillDays.Add(skill, new List<ISkillDay> { skillDay });

            _view.Stub(x => x.SelectedSkill).Return(skill);

            IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
            
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "SkillTest - {0}", _target.IntradayDate.ToShortDateString()), _target.ChartIntradayDescription);
            Assert.AreEqual(2, skillStaffPeriods.Count);
        }

        [Test]
        public void VerifyPrepareSkillIntradayCollectionWithNotContainedSkill()
        {
            ISkill skill = MockRepository.GenerateMock<ISkill>();
            _schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
            
            _view.Stub(x => x.SelectedSkill).Return(skill);
            
            IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
            Assert.AreEqual(0, skillStaffPeriods.Count);
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandler()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();

            _view.Stub(x => x.InvokeRequired).Return(false);
            
            _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
            _statisticCommand.AssertWasCalled(x => x.Execute(eventMessage));
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandlerInvokeRequired()
        {
            _view.Stub(x => x.InvokeRequired).Return(true);
            
            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(new EventMessage()));

            _view.AssertWasCalled(x => x.BeginInvoke(new Action<object, EventMessageArgs>(_target.OnEventForecastDataMessageHandler), new object[] {}), o => o.IgnoreArguments());
        }

        [Test]
        public void VerifyOnEventScheduleMessageHandlerInvokeRequired()
        {
            _view.Stub(x => x.InvokeRequired).Return(true);
            
            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(new EventMessage()));

            _view.AssertWasCalled(x => x.BeginInvoke(new Action<object, EventMessageArgs>(_target.OnEventScheduleMessageHandler), new object(), new object()), o => o.IgnoreArguments());
        }

        [Test]
        public void VerifyOnEventMeetingMessageHandlerInvokeRequired()
        {
            _view.Stub(x => x.InvokeRequired).Return(true);

            _target.OnEventMeetingMessageHandler(null, new EventMessageArgs(new EventMessage()));

            _view.AssertWasCalled(x => x.BeginInvoke(new Action<object, EventMessageArgs>(_target.OnEventMeetingMessageHandler), new object(), new object()), o => o.IgnoreArguments());
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandlerInvokeRequired()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            
            _view.Stub(x => x.InvokeRequired).Return(true);
            
            _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));

            _view.AssertWasCalled(x => x.BeginInvoke(new EventHandler<EventMessageArgs>(_target.OnEventStatisticMessageHandler), new object[] { }), o => o.IgnoreArguments());
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            
            _view.Stub(x => x.InvokeRequired).Return(false);
            eventMessage.Stub(x => x.ModuleId).Return(_target.InitiatorId);

            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(eventMessage));

            _forecastCommand.AssertWasNotCalled(x => x.Execute(eventMessage));
        }

        [Test]
        public void VerifyOnEventMeetingMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            _view.Stub(x => x.InvokeRequired).Return(false);
            eventMessage.Stub(x => x.ModuleId).Return(_target.InitiatorId);

            _target.OnEventMeetingMessageHandler(null, new EventMessageArgs(eventMessage));

            _meetingCommand.AssertWasNotCalled(x => x.Execute(eventMessage));
        }

        [Test]
        public void VerifyOnEventScheduleMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            _view.Stub(x => x.InvokeRequired).Return(false);
            eventMessage.Stub(x => x.ModuleId).Return(_target.InitiatorId);

            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(eventMessage));

            _scheduleCommand.AssertWasNotCalled(x => x.Execute(eventMessage));
        }

        [Test]
        public void VerifyOnEventExternalAgentStateMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            eventMessage.Stub(x => x.ModuleId).Return(_target.InitiatorId);

            _target.OnEventActualAgentStateMessageHandler(null, new EventMessageArgs(eventMessage));
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = MockRepository.GenerateMock<IEventMessage>();
            _view.Stub(x => x.InvokeRequired).Return(false);
            eventMessage.Stub(x => x.ModuleId).Return(_target.InitiatorId);

            _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));

            _statisticCommand.AssertWasNotCalled(x => x.Execute(eventMessage));
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandler()
        {
            var message = new EventMessage();

            _view.Stub(x => x.InvokeRequired).Return(false);
            
            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(message));

            _forecastCommand.AssertWasCalled(x => x.Execute(message));
        }

        [Test]
        public void VerifyOnEventMeetingMessageHandler()
        {
            var message = new EventMessage
            {
                InterfaceType = typeof(IMeetingChangedEntity),
                DomainObjectId = Guid.NewGuid(),
                DomainUpdateType = DomainUpdateType.NotApplicable
            };

            _view.Stub(x => x.InvokeRequired).Return(false);

            _target.OnEventMeetingMessageHandler(null, new EventMessageArgs(message));

            _meetingCommand.AssertWasCalled(x => x.Execute(message));
        }

        [Test]
        public void VerifyOnEventScheduleDataMessageHandler()
        {
            var message = new EventMessage
            {
                InterfaceType = typeof(IScheduleChangedEvent),
                DomainObjectId = Guid.NewGuid(),
                DomainUpdateType = DomainUpdateType.NotApplicable
            };

            _view.Stub(x => x.InvokeRequired).Return(false);
            
            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(message));
            
            _scheduleCommand.AssertWasCalled(x => x.Execute(message));
        }

        [Test]
        public void VerifyOnLoadWithMoreThanOneHundredPeople()
        {
            var uow = MockRepository.GenerateMock<IUnitOfWork>();

            var agentState = new ActualAgentState();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

            _rtaStateHolder.Stub(r => r.ActualAgentStates)
                           .Return(new Dictionary<Guid, IActualAgentState> {{Guid.NewGuid(), agentState}});
            _repositoryFactory.Stub(x => x.CreateStatisticRepository())
                              .Return(_statisticRepository);
            _statisticRepository.Stub(x => x.LoadActualAgentState(null)).Return(new List<IActualAgentState> {agentState});

            Enumerable.Range(0, 101)
                      .ForEach(_ => _schedulerStateHolder.FilteredAgentsDictionary.Add(Guid.NewGuid(), _persons[0]));

            _schedulerStateHolder.RequestedPeriod =
                new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2), DateOnly.Today.AddDays(2)),
                                                   TimeZoneInfo.Utc);
            _target.Initialize();

            Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
            _schedulingResultLoader.AssertWasCalled(x => x.LoadWithIntradayData(uow));
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, null), o=> o.IgnoreArguments().Repeat.Twice());
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, Guid.Empty, typeof(Scenario), null, DateTime.UtcNow, DateTime.UtcNow), o => o.IgnoreArguments());
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, null, DateTime.UtcNow, DateTime.UtcNow), o => o.IgnoreArguments().Repeat.Times(2));
        }

        [Test]
        public void VerifyOnLoad()
        {
            var uow = MockRepository.GenerateMock<IUnitOfWork>();
            var agentState = new ActualAgentState();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
            
            _rtaStateHolder.Stub(r => r.ActualAgentStates)
                           .Return(new Dictionary<Guid, IActualAgentState> { { Guid.NewGuid(), agentState } });
            _repositoryFactory.Stub(x => x.CreateStatisticRepository())
                              .Return(_statisticRepository);
            _statisticRepository.Stub(x => x.LoadActualAgentState(null)).Return(new List<IActualAgentState>{agentState});
            _schedulerStateHolder.RequestedPeriod =
                new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2), DateOnly.Today.AddDays(2)),
                                                   TimeZoneInfo.Utc);
            _target.Initialize();

            Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
            _schedulingResultLoader.AssertWasCalled(x => x.LoadWithIntradayData(uow));
        }

		private static void MyEventHandler(object sender, EventMessageArgs e)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyOnLoadWithoutRtaEnabled()
		{
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var authorization = MockRepository.GenerateMock<IPrincipalAuthorization>();

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)).Return(false);
			
			using (new CustomAuthorizationContext(authorization))
			{
				_target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder,
												_eventAggregator, null, _unitOfWorkFactory, _repositoryFactory, _differenceService,
												_statisticCommand, _forecastCommand, _scheduleCommand, _meetingCommand, _loadStatisticCommand);
				_target.Initialize();
			}

			Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
            _schedulingResultLoader.AssertWasCalled(x => x.LoadWithIntradayData(uow));
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, null), o => o.IgnoreArguments().Repeat.Twice());
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, Guid.Empty, typeof(Scenario), null, DateTime.UtcNow, DateTime.UtcNow), o => o.IgnoreArguments());
            _messageBroker.AssertWasCalled(x => x.RegisterEventSubscription(MyEventHandler, null, DateTime.UtcNow, DateTime.UtcNow), o => o.IgnoreArguments());
        }

        [Test]
        public void VerifySave()
        {
            IUnitOfWork uow = MockRepository.GenerateMock<IUnitOfWork>();
            IScheduleDictionary scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
						scheduleDictionary.Expect(x => x.Values).Return(new[] { MockRepository.GenerateMock<IScheduleRange, IUnvalidatedScheduleRangeUpdate>() });

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(new DifferenceCollection<IPersistableScheduleData> { new DifferenceCollectionItem<IPersistableScheduleData>() });
            uow.Expect(x => x.PersistAll(_target)).Return(new List<IRootChangeInfo>()).Repeat.Once();
            uow.Expect(x => x.PersistAll(_target)).Throw(new OptimisticLockException());

            _target.Save();
            _target.Save();

            _schedulingResultLoader.AssertWasCalled(x => x.ReloadScheduleData(uow));
            _view.AssertWasCalled(x => x.RefreshRealTimeScheduleControls());
            _view.AssertWasCalled(x => x.ToggleSchedulePartModified(true));

            _view.AssertWasCalled(x => x.DrawSkillGrid());
            _view.AssertWasCalled(x => x.UpdateShiftEditor(new List<IScheduleDay>()));
            _view.AssertWasCalled(x => x.DisableSave());
        }

        [Test]
        public void VerifyCheckIfUserWantsToSaveUnsavedData()
        {
            IUnitOfWork uow = MockRepository.GenerateMock<IUnitOfWork>();
            IScheduleDictionary scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>();

	        scheduleDictionary.Expect(x => x.Values).Return(new[]{MockRepository.GenerateMock<IScheduleRange, IUnvalidatedScheduleRangeUpdate>()});

            scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollection).Repeat.Once();
            scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollection).Repeat.Once();
            _view.Stub(x => x.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Cancel).Repeat.Once();
            scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollection).Repeat.Once();
            _view.Stub(x => x.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.No).Repeat.Once();
            scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollection).Repeat.Once();
            _view.Stub(x => x.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Yes).Repeat.Once();
			scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(new DifferenceCollection<IPersistableScheduleData>()).Repeat.Once();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Once();
            uow.Stub(x => x.PersistAll(_target)).Return(new List<IRootChangeInfo>()).Repeat.Once();

            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
			differenceCollection.Add(new DifferenceCollectionItem<IPersistableScheduleData>());
            Assert.IsTrue(_target.CheckIfUserWantsToSaveUnsavedData());
            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
            _view.AssertWasCalled(x => x.UpdateShiftEditor(new List<IScheduleDay>()));
            _view.AssertWasCalled(x => x.DisableSave());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_period, _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
            Assert.IsNotNull(_schedulerStateHolder.SchedulingResultState);
            Assert.AreEqual(0, _schedulerStateHolder.SchedulingResultState.PersonsInOrganization.Count);
            Assert.IsTrue(string.IsNullOrEmpty(_target.ChartIntradayDescription));
            Assert.AreEqual(_period.StartDate, _target.IntradayDate);
            Assert.IsNotNull(_target.RtaStateHolder);
            Assert.AreNotEqual(Guid.Empty, _target.InitiatorId);
            Assert.IsTrue(_target.RealTimeAdherenceEnabled);
            Assert.IsTrue(_target.EarlyWarningEnabled);
        }

        [Test]
        public void VerifyCanSetIntradayDate()
        {
            _target.IntradayDate = _period.EndDate;
            
            Assert.AreEqual(_period.EndDate, _target.IntradayDate);
            _view.AssertWasCalled(x => x.DrawSkillGrid());
        }

        [Test]
        public void VerifyPrepareChartDescription()
        {
            string description = IntradayPresenter.PrepareChartDescription("{0} - {1}", "test1", "test2");
            Assert.AreEqual("test1 - test2", description);
        }

        [Test]
        public void VerifyCanGetAgentStateViewAdapterCollection()
        {
            var stategroups = new ReadOnlyCollection<IRtaStateGroup>(new List<IRtaStateGroup> { new RtaStateGroup("test", true, true) });
            var model = MockRepository.GenerateMock<IDayLayerViewModel>();
            _rtaStateHolder.Stub(x => x.RtaStateGroups).Return(stategroups);
            string test = "";
            foreach (var adaper in _target.CreateAgentStateViewAdapterCollection(model))
            {
                test = adaper.StateGroup.Name;
            }
            Assert.AreEqual("test", test);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
        }
    }
}
