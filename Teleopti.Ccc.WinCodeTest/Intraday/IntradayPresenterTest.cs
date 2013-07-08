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
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
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
        private MockRepository _mocks;
        private IList<IPerson> _persons;
        private IIntradayView _view;
        private IMessageBroker _messageBroker;
        private IRtaStateHolder _rtaStateHolder;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISchedulingResultLoader _schedulingResultLoader;
        private ISchedulerStateHolder _schedulerStateHolder;
		private IStatisticRepository _statisticRepository;
        private IRepositoryFactory _repositoryFactory;
        private IScenario _scenario;
        private IEventAggregator _eventAggregator;
        private IScheduleDictionarySaver _scheduleDictionarySaver;
        private IScheduleRepository _scheduleRepository;
        private OnEventScheduleMessageCommand _scheduleCommand;
        private OnEventForecastDataMessageCommand _forecastCommand;
        private OnEventStatisticMessageCommand _statisticCommand;
        private LoadStatisticsAndActualHeadsCommand _loadStatisticCommand;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _eventAggregator = new EventAggregator();
            _view = _mocks.DynamicMock<IIntradayView>();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _persons = new List<IPerson> { PersonFactory.CreatePerson() };
            _schedulingResultLoader = _mocks.DynamicMock<ISchedulingResultLoader>();
            _messageBroker = _mocks.StrictMock<IMessageBroker>();
            _rtaStateHolder = _mocks.DynamicMock<IRtaStateHolder>();
            _unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.DynamicMock<IRepositoryFactory>();
            _scheduleDictionarySaver = _mocks.DynamicMock<IScheduleDictionarySaver>();
            _scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
            _schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipal.Current.Regional.TimeZone), _persons);
			_statisticRepository = _mocks.DynamicMock<IStatisticRepository>();

            _scheduleCommand = _mocks.DynamicMock<OnEventScheduleMessageCommand>();
            _forecastCommand = _mocks.DynamicMock<OnEventForecastDataMessageCommand>();
            _statisticCommand = _mocks.DynamicMock<OnEventStatisticMessageCommand>();
            _loadStatisticCommand = _mocks.DynamicMock<LoadStatisticsAndActualHeadsCommand>((IStatisticRepository)null);

            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

            _mocks.Replay(_schedulingResultLoader);
            _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker,
                                            _rtaStateHolder, _eventAggregator,
                                            _scheduleDictionarySaver, _scheduleRepository, _unitOfWorkFactory,
                                            _repositoryFactory, _statisticCommand, _forecastCommand,
                                            _scheduleCommand, _loadStatisticCommand);

            _mocks.Verify(_schedulingResultLoader);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyHandlesDateIfTodayIsIncludedInSelection()
        {
            _mocks.BackToRecord(_schedulingResultLoader);
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_periodNow, TeleoptiPrincipal.Current.Regional.TimeZone), _persons)).Repeat.AtLeastOnce();

            _mocks.Replay(_schedulingResultLoader);
            _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder, _eventAggregator,
                                            _scheduleDictionarySaver, _scheduleRepository, _unitOfWorkFactory,
                                            _repositoryFactory, _statisticCommand, _forecastCommand,
                                            _scheduleCommand, _loadStatisticCommand);
            Assert.AreEqual(DateOnly.Today, _target.IntradayDate);
            Assert.IsFalse(_target.HistoryOnly);
            _mocks.Verify(_schedulingResultLoader);
        }

        [Test]
        public void VerifyMultiplicatorDefinitionSetsCanBeRead()
        {
            _mocks.BackToRecord(_schedulingResultLoader);

            var multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            Expect.Call(_schedulingResultLoader.MultiplicatorDefinitionSets).Return(multiplicatorDefinitionSets);

            _mocks.Replay(_schedulingResultLoader);

            Assert.AreEqual(multiplicatorDefinitionSets, _target.MultiplicatorDefinitionSets);

            _mocks.Verify(_schedulingResultLoader);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyPrepareSkillIntradayCollection()
        {
            var period = _schedulerStateHolder.RequestedPeriod.Period();
            var smallPeriod = new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15));
            var skill = _mocks.StrictMock<ISkill>();
            var skillDay = _mocks.DynamicMock<ISkillDay>();

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

            Expect.Call(skill.Name).Return("SkillTest").Repeat.AtLeastOnce();
            Expect.Call(skillDay.SkillStaffPeriodCollection).Return(
                new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3 }));

            _schedulerStateHolder.SchedulingResultState.Skills.Add(skill);
            _schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
            _schedulerStateHolder.SchedulingResultState.SkillDays.Add(skill, new List<ISkillDay> { skillDay });

            Expect.Call(_view.SelectedSkill).Return(skill);

            _mocks.BackToRecord(_schedulingResultLoader);
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
            _mocks.VerifyAll();
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "SkillTest - {0}", _target.IntradayDate.ToShortDateString()), _target.ChartIntradayDescription);
            Assert.AreEqual(2, skillStaffPeriods.Count);
        }

        [Test]
        public void VerifyPrepareSkillIntradayCollectionWithNotContainedSkill()
        {
            ISkill skill = _mocks.StrictMock<ISkill>();
            _schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
            Expect.Call(_view.SelectedSkill).Return(skill);

            _mocks.BackToRecord(_schedulingResultLoader);
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder);

            _mocks.ReplayAll();
            IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
            _mocks.VerifyAll();
            Assert.AreEqual(0, skillStaffPeriods.Count);
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandler()
        {
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            using (_mocks.Record())
            {
                Expect.Call(_view.InvokeRequired).Return(false);
                Expect.Call(eventMessage.ModuleId).Return(Guid.Empty);
                Expect.Call(() => _statisticCommand.Execute(eventMessage));
            }

            using (_mocks.Playback())
            {
                _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
            }
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandlerInvokeRequired()
        {
            IAsyncResult result = _mocks.StrictMock<IAsyncResult>();
            Expect.Call(_view.InvokeRequired).Return(true);
            Expect.Call(_view.BeginInvoke(new Action<object, EventMessageArgs>(_target.OnEventForecastDataMessageHandler), new object[] { })).IgnoreArguments().Return(result);
            _mocks.ReplayAll();
            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(new EventMessage()));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventScheduleMessageHandlerInvokeRequired()
        {
            IAsyncResult result = _mocks.StrictMock<IAsyncResult>();
            Expect.Call(_view.InvokeRequired).Return(true);
            Expect.Call(_view.BeginInvoke(new Action<object, EventMessageArgs>(_target.OnEventScheduleMessageHandler), new object(), new object())).IgnoreArguments().Return(result);
            _mocks.ReplayAll();
            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(new EventMessage()));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandlerInvokeRequired()
        {
            IAsyncResult result = _mocks.StrictMock<IAsyncResult>();
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            Expect.Call(_view.InvokeRequired).Return(true);
            Expect.Call(_view.BeginInvoke(new EventHandler<EventMessageArgs>(_target.OnEventStatisticMessageHandler), new object[] { })).IgnoreArguments().Return(result);
            _mocks.ReplayAll();
            _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
            _mocks.ReplayAll();
            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(eventMessage));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventScheduleMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
            _mocks.ReplayAll();
            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(eventMessage));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventExternalAgentStateMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
            _mocks.ReplayAll();
            _target.OnEventActualAgentStateMessageHandler(null, new EventMessageArgs(eventMessage));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventStatisticMessageHandlerSameModuleId()
        {
            IEventMessage eventMessage = _mocks.StrictMock<IEventMessage>();
            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
            _mocks.ReplayAll();
            _target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandler()
        {
            var message = new EventMessage();

            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(() => _forecastCommand.Execute(message));

            _mocks.ReplayAll();
            _target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(message));

            _mocks.VerifyAll();
        }
		
        [Test]
        public void VerifyOnEventScheduleDataMessageHandlerAssignment()
        {
            var message = new EventMessage
            {
                InterfaceType = typeof(IPersonAssignment),
                DomainObjectId = Guid.NewGuid(),
                DomainUpdateType = DomainUpdateType.Insert
            };

            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(() => _scheduleCommand.Execute(message));

            _mocks.ReplayAll();

            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(message));

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnEventScheduleDataMessageHandlerDeleteAssignment()
        {
            var idFromBroker = Guid.NewGuid();
            var message = new EventMessage
            {
                InterfaceType = typeof(IPersonAssignment),
                DomainObjectId = idFromBroker,
                DomainUpdateType = DomainUpdateType.Delete
            };

            Expect.Call(_view.InvokeRequired).Return(false);
            Expect.Call(() => _scheduleCommand.Execute(message));

            _mocks.ReplayAll();
            _target.OnEventScheduleMessageHandler(null, new EventMessageArgs(message));

            _mocks.VerifyAll();
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        //[Test] 
		//public void VerifyOnEventExternalAgentStateMessageHandler()
        //{
        //    var uow = mocks.DynamicMock<IUnitOfWork>();
        //    var eventMessage = mocks.StrictMock<IEventMessage>();
        //    var authorization = mocks.StrictMock<IPrincipalAuthorization>();
        //    var stateHolder = new SchedulerStateHolder(_scenario,
        //                                               new DateOnlyPeriodAsDateTimePeriod(_periodNow,
        //                                                                                  TeleoptiPrincipal.Current.Regional.
        //                                                                                    TimeZone), _persons);
        //    var period = stateHolder.RequestedPeriod.Period();

        //    //IExternalAgentState externalAgentState = new ExternalAgentState("001", "AUX2", TimeSpan.Zero, DateTime.MinValue, Guid.NewGuid(), 1, DateTime.UtcNow, false);
        //    //byte[] data = new ExternalAgentStateEncoder().Encode(externalAgentState);
        //    //mocks.BackToRecord(_schedulingResultLoader);
        //    createRealTimeAdherenceInitializeExpectation();
        //    createRtaStateHolderExpectation();

        //    Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();

        //    Expect.Call(()=>_messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null)).IgnoreArguments().Repeat.Times(3);
        //    Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
        //                                                            period.EndDateTime)).IgnoreArguments().Repeat.Times(3);
        //    Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, Guid.Empty, typeof(IActualAgentState), period.StartDateTime,
        //                                                            period.EndDateTime)).IgnoreArguments().Repeat.Times(3);

        //    //Expect.Call(_repositoryFactory.CreateStatisticRepository()).Return(_statisticRepository);

        //    //Expect.Call(_statisticRepository.LoadRtaAgentStates(
        //    //    _schedulerStateHolder.RequestedPeriod.Period(), _rtaStateHolder.ExternalLogOnPersons)).IgnoreArguments().Repeat.Twice().
        //    //    Return(new Collection<IExternalAgentState>());

        //    Expect.Call(eventMessage.ModuleId).Return(Guid.Empty);
        //    //Expect.Call(_rtaStateHolder.FilteredPersons).Return(new List<IPerson>());
        //    //Expect.Call(eventMessage.DomainObject).Return(data);

        //    Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning)).Return(true);
        //    Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)).Return(true);
        //    //Expect.Call(_schedulingResultLoader.SchedulerState).Return(stateHolder).Repeat.AtLeastOnce();
        //    //Expect.Call(()=>_schedulingResultLoader.LoadWithIntradayData(uow));



        //    mocks.ReplayAll();
        //    using (new CustomAuthorizationContext(authorization))
        //    {
        //        _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder,
        //                                        _eventAggregator, null, null, _unitOfWorkFactory, _repositoryFactory,
        //                                        _statisticCommand, _forecastCommand, _scheduleCommand, _loadStatisticCommand);
        //        _target.Initialize();
        //        bool updateMessageReceived = false;
        //        _target.ExternalAgentStateReceived += (x, y) =>
        //                                                  {
        //                                                      updateMessageReceived = true;
        //                                                  };
        //        _target.OnEventActualAgentStateMessageHandler(null, new EventMessageArgs(eventMessage));

        //        Assert.That(() => updateMessageReceived, Is.True.After(10000, 10));
        //    }
        //    mocks.VerifyAll();
        //}
		
		[Test]
        public void VerifyOnLoadWithMoreThanOneHundredPeople()
        {
			_mocks.BackToRecord(_schedulingResultLoader);
            var uow = _mocks.DynamicMock<IUnitOfWork>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
			Expect.Call(() => _schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();

            CreateRtaStateHolderExpectation();
			CreateRealTimeAdherenceInitializeExpectation();

			_rtaStateHolder.Expect(r => r.ActualAgentStates)
			               .Return(new Dictionary<Guid, IActualAgentState> {{Guid.NewGuid(), new ActualAgentState()}}).IgnoreArguments();
			Expect.Call(() => _messageBroker.RegisterEventSubscription(MyEventHandler, null)).IgnoreArguments().Repeat.Twice();
        	Expect.Call(
        		() => _messageBroker.RegisterEventSubscription(MyEventHandler, null, DateTime.UtcNow, DateTime.UtcNow)).
				IgnoreArguments().Repeat.Times(3);

			_mocks.ReplayAll();

		    Enumerable.Range(0, 101)
		              .ForEach(_ => _schedulerStateHolder.FilteredPersonDictionary.Add(Guid.NewGuid(), _persons[0]));
		    _schedulerStateHolder.RequestedPeriod =
		        new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2), DateOnly.Today.AddDays(2)),
		                                           TimeZoneInfo.Utc);
            _target.Initialize();

            _mocks.VerifyAll();
            Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
        }

        [Test]
        public void VerifyOnLoad()
        {
            _mocks.BackToRecord(_schedulingResultLoader);
            var uow = _mocks.DynamicMock<IUnitOfWork>();

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
            Expect.Call(() => _schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();

            CreateRtaStateHolderExpectation();
            CreateRealTimeAdherenceInitializeExpectation();

            _rtaStateHolder.Expect(r => r.ActualAgentStates)
                           .Return(new Dictionary<Guid, IActualAgentState> { { Guid.NewGuid(), new ActualAgentState() } }).IgnoreArguments();
            Expect.Call(() => _messageBroker.RegisterEventSubscription(MyEventHandler, null)).IgnoreArguments().Repeat.Twice();
            Expect.Call(
                () => _messageBroker.RegisterEventSubscription(MyEventHandler, null, DateTime.UtcNow, DateTime.UtcNow)).
                IgnoreArguments().Repeat.Twice();
            Expect.Call(
                () => _messageBroker.RegisterEventSubscription(MyEventHandler, Guid.Empty, null, DateTime.UtcNow, DateTime.UtcNow)).
                IgnoreArguments();

            _mocks.ReplayAll();

            _schedulerStateHolder.RequestedPeriod =
                new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2), DateOnly.Today.AddDays(2)),
                                                   TimeZoneInfo.Utc);
            _target.Initialize();

            _mocks.VerifyAll();
            Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
        }

		private void CreateRealTimeAdherenceInitializeExpectation()
		{
			var actualAgentState = new ActualAgentState();
			var actualAgentList = new List<IActualAgentState> { actualAgentState };

			Expect.Call(_repositoryFactory.CreateStatisticRepository()).Return(_statisticRepository).Repeat.Any();
			Expect.Call(_statisticRepository.LoadActualAgentState(_persons)).IgnoreArguments().Return(actualAgentList);
			Expect.Call(() => _rtaStateHolder.SetActualAgentState(actualAgentState));
		}

		private void CreateRtaStateHolderExpectation()
		{
			Expect.Call(_rtaStateHolder.Initialize);
			_schedulerStateHolder.FilteredPersonDictionary.Add(Guid.Empty, _persons.First());
			Expect.Call(_target.SchedulerStateHolder).IgnoreArguments().Return(_schedulerStateHolder);
			Expect.Call(() => _rtaStateHolder.SetFilteredPersons(_schedulerStateHolder.FilteredPersonDictionary.Values))
				.IgnoreArguments();
			Expect.Call(_rtaStateHolder.VerifyDefaultStateGroupExists);
			
		}

		private static void MyEventHandler(object sender, EventMessageArgs e)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyOnLoadWithoutRtaEnabled()
		{
			_mocks.BackToRecord(_schedulingResultLoader);

			var uow = _mocks.DynamicMock<IUnitOfWork>();
			var authorization = _mocks.StrictMock<IPrincipalAuthorization>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
			Expect.Call(() => _schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();
			Expect.Call(_rtaStateHolder.Initialize);
			Expect.Call(_target.SchedulerStateHolder).IgnoreArguments().Return(_schedulerStateHolder);
			Expect.Call(() => _rtaStateHolder.SetFilteredPersons(_schedulerStateHolder.FilteredPersonDictionary.Values))
				.IgnoreArguments();
			Expect.Call(() => _messageBroker.RegisterEventSubscription(MyEventHandler, null)).IgnoreArguments().Repeat.Twice();
			Expect.Call(
				() => _messageBroker.RegisterEventSubscription(MyEventHandler, null, DateTime.UtcNow, DateTime.UtcNow)).
				IgnoreArguments().Repeat.Twice();

			Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning)).Return(true);
			Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)).Return(false);
			
			_mocks.ReplayAll();
			using (new CustomAuthorizationContext(authorization))
			{
				_target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder,
												_eventAggregator, null, null, _unitOfWorkFactory, _repositoryFactory,
												_statisticCommand, _forecastCommand, _scheduleCommand, _loadStatisticCommand);
				_target.Initialize();
			}
			_mocks.VerifyAll();

			Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
		}

        //[Test] And this
        //public void VerifyOnLoadWithPersons()
        //{
        //    mocks.BackToRecord(_schedulingResultLoader);

        //    var uow = mocks.DynamicMock<IUnitOfWork>();
        //    var period = _schedulerStateHolder.RequestedPeriod.Period();
        //    var actualAgentState = new ActualAgentState();
        //    var actualAgentList = new List<IActualAgentState> { actualAgentState };

        //    createRealTimeAdherenceInitializeExpectation();
        //    createRtaStateHolderExpectation();

        //    Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();

        //    Expect.Call(() => _schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();

        //    Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, typeof (IStatisticTask)))
        //        .
        //        IgnoreArguments().Repeat.Times(2);
        //    Expect.Call(
        //        () => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
        //                                                       period.EndDateTime)).IgnoreArguments().Repeat.Times(3);

        //    Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
        //    Expect.Call(_statisticRepository.LoadActualAgentState(new List<IPerson>())).IgnoreArguments().Return(
        //        new List<IActualAgentState>());
        //    // Expected #0, Actual #1 error:
        //    Expect.Call(_statisticRepository.LoadRtaAgentStates(
        //        period, new List<ExternalLogOnPerson>())).IgnoreArguments().Return(new List<IExternalAgentState>());

        //    mocks.ReplayAll();

        //    _target.Initialize();

        //    mocks.VerifyAll();

        //    Assert.IsTrue(_schedulerStateHolder.AllPermittedPersons.Contains(_persons[0]));
        //}

        [Test]
        public void VerifySave()
        {
            IUnitOfWork uow = _mocks.StrictMock<IUnitOfWork>();
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            _mocks.BackToRecord(_schedulingResultLoader);
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.Any();

            using (_mocks.Ordered())
            {
                Expect.Call(_view.UpdateFromEditor);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

                CreateBasicExpectationForSave(uow, scheduleDictionary);

                Expect.Call(uow.PersistAll(_target)).Return(new List<IRootChangeInfo>());
                Expect.Call(uow.Dispose);
                Expect.Call(() => _view.UpdateShiftEditor(new List<IScheduleDay>()));
                Expect.Call(_view.DisableSave);
                Expect.Call(_view.UpdateFromEditor);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                CreateBasicExpectationForSave(uow, scheduleDictionary);
                Expect.Call(uow.PersistAll(_target)).Throw(new OptimisticLockException());
                Expect.Call(uow.Dispose);
                Expect.Call(() => _view.ShowInformationMessage("", "")).IgnoreArguments();

                Expect.Call(() => _view.ToggleSchedulePartModified(false));

                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(() => _schedulingResultLoader.ReloadScheduleData(uow));
                Expect.Call(uow.Dispose);
                Expect.Call(_view.RefreshRealTimeScheduleControls);
                Expect.Call(() => _view.ToggleSchedulePartModified(true));

                Expect.Call(_view.DrawSkillGrid);
                Expect.Call(() => _view.UpdateShiftEditor(new List<IScheduleDay>()));
                Expect.Call(_view.DisableSave);
            }

            _mocks.ReplayAll();
            _target.Save();
            _target.Save();

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCheckIfUserWantsToSaveUnsavedData()
        {
            IUnitOfWork uow = _mocks.StrictMock<IUnitOfWork>();
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            var differenceCollection = new DifferenceCollection<IPersistableScheduleData>();

            _mocks.BackToRecord(_schedulingResultLoader);
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.Any();
            using (_mocks.Ordered())
            {
                Expect.Call(scheduleDictionary.DifferenceSinceSnapshot()).Return(differenceCollection);
                Expect.Call(scheduleDictionary.DifferenceSinceSnapshot()).Return(differenceCollection);
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Cancel);
                Expect.Call(scheduleDictionary.DifferenceSinceSnapshot()).Return(differenceCollection);
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.No);
                Expect.Call(scheduleDictionary.DifferenceSinceSnapshot()).Return(differenceCollection);
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Yes);
                Expect.Call(_view.UpdateFromEditor);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                CreateBasicExpectationForSave(uow, scheduleDictionary);
                Expect.Call(uow.PersistAll(_target)).Return(new List<IRootChangeInfo>());
                Expect.Call(uow.Dispose);
                Expect.Call(() => _view.UpdateShiftEditor(new List<IScheduleDay>()));
                Expect.Call(_view.DisableSave);
            }

            _mocks.ReplayAll();
            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
            differenceCollection.Add(new DifferenceCollectionItem<IPersistableScheduleData>());
            Assert.IsTrue(_target.CheckIfUserWantsToSaveUnsavedData());
            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
            Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());

            _mocks.VerifyAll();
        }

        private void CreateBasicExpectationForSave(IUnitOfWork uow, IScheduleDictionary scheduleDictionary)
        {
            var changes = new DifferenceCollection<IPersistableScheduleData>();

            Expect.Call(() => uow.Reassociate(_schedulerStateHolder.CommonStateHolder.Activities));
            Expect.Call(() => uow.Reassociate(_schedulerStateHolder.CommonStateHolder.Absences));
            Expect.Call(() => uow.Reassociate(_schedulerStateHolder.CommonStateHolder.ShiftCategories));

            Expect.Call(() => uow.Reassociate(new List<IPerson>()));

            var multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            Expect.Call(_schedulingResultLoader.MultiplicatorDefinitionSets).Return(multiplicatorDefinitionSets);
            Expect.Call(() => uow.Reassociate(multiplicatorDefinitionSets));

            Expect.Call(scheduleDictionary.DifferenceSinceSnapshot()).Return(changes);
            Expect.Call(_scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository, changes)).Return(null);
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
            Assert.AreNotEqual(Guid.Empty, _target.ModuleId);
            Assert.IsTrue(_target.RealTimeAdherenceEnabled);
            Assert.IsTrue(_target.EarlyWarningEnabled);
        }

        [Test]
        public void VerifyCanSetIntradayDate()
        {
            using (_mocks.Record())
            {
                _view.DrawSkillGrid();
                LastCall.Repeat.Once();
            }
            using (_mocks.Playback())
            {
                _target.IntradayDate = _period.EndDate;
            }
            Assert.AreEqual(_period.EndDate, _target.IntradayDate);
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
            var model = _mocks.DynamicMock<IDayLayerViewModel>();
            Expect.Call(_rtaStateHolder.RtaStateGroups).Return(stategroups);
            _mocks.ReplayAll();
            string test = "";
            foreach (var adaper in _target.CreateAgentStateViewAdapterCollection(model))
            {
                test = adaper.StateGroup.Name;
            }
            _mocks.VerifyAll();
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
			_mocks.BackToRecord(_messageBroker);
			_mocks.BackToRecord(_rtaStateHolder);
			_messageBroker.UnregisterEventSubscription(_target.OnEventForecastDataMessageHandler);
			//_messageBroker.UnregisterEventSubscription(_target.OnEventScheduleMessageHandler);
			//_messageBroker.UnregisterEventSubscription(_target.OnEventStatisticMessageHandler);
			//_messageBroker.UnregisterEventSubscription(_target.OnEventActualAgentStateMessageHandler);
			LastCall.IgnoreArguments().Repeat.Any();
			_mocks.Replay(_rtaStateHolder);
			_mocks.Replay(_messageBroker);
			_target.Dispose();
			_mocks.Verify(_messageBroker);
			_mocks.Verify(_rtaStateHolder);
        }
    }
}
