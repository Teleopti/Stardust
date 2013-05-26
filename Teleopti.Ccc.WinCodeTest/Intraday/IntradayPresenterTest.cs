using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class IntradayPresenterTest : IDisposable
	{
		private IntradayPresenter _target;
        private readonly DateOnlyPeriod _period = new DateOnlyPeriod(2008, 10, 20,2008,10,20);
		private readonly DateOnlyPeriod _periodNow = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));
		private MockRepository mocks;
		private IList<IPerson> _persons;
		private IIntradayView _view;
		private IMessageBroker _messageBroker;
		private IRtaStateHolder _rtaStateHolder;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ISchedulingResultLoader _schedulingResultLoader;
		private ISchedulerStateHolder _schedulerStateHolder;
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
			mocks = new MockRepository();
			_eventAggregator = new EventAggregator();
			_view = mocks.StrictMock<IIntradayView>();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_persons = new List<IPerson>{PersonFactory.CreatePerson()};
			_schedulingResultLoader = mocks.DynamicMock<ISchedulingResultLoader>();
			_messageBroker = mocks.StrictMock<IMessageBroker>();
			_rtaStateHolder = mocks.StrictMock<IRtaStateHolder>();
			_unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
			_repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
			_scheduleDictionarySaver = mocks.StrictMock<IScheduleDictionarySaver>();
			_scheduleRepository = mocks.StrictMock<IScheduleRepository>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipal.Current.Regional.TimeZone), _persons);

			_scheduleCommand = mocks.DynamicMock<OnEventScheduleMessageCommand>();
			_forecastCommand = mocks.DynamicMock<OnEventForecastDataMessageCommand>();
			_statisticCommand = mocks.DynamicMock<OnEventStatisticMessageCommand>();
			_loadStatisticCommand = mocks.DynamicMock<LoadStatisticsAndActualHeadsCommand>((IStatisticRepository)null);

			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
			
			mocks.Replay(_schedulingResultLoader);
	        _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker,
	                                        _rtaStateHolder, _eventAggregator,
	                                        _scheduleDictionarySaver, _scheduleRepository, _unitOfWorkFactory,
	                                        _repositoryFactory, _statisticCommand, _forecastCommand,
	                                        _scheduleCommand, _loadStatisticCommand);
			mocks.Verify(_schedulingResultLoader);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyHandlesDateIfTodayIsIncludedInSelection()
		{
			mocks.BackToRecord(_schedulingResultLoader);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(new SchedulerStateHolder(_scenario,new DateOnlyPeriodAsDateTimePeriod(_periodNow,TeleoptiPrincipal.Current.Regional.TimeZone), _persons)).Repeat.AtLeastOnce();

			mocks.Replay(_schedulingResultLoader);
		    _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder, _eventAggregator,
		                                    _scheduleDictionarySaver, _scheduleRepository, _unitOfWorkFactory,
                                            _repositoryFactory, _statisticCommand, _forecastCommand,
                                            _scheduleCommand, _loadStatisticCommand);
			Assert.AreEqual(DateOnly.Today,_target.IntradayDate);
			Assert.IsFalse(_target.HistoryOnly);
			mocks.Verify(_schedulingResultLoader);
		}

		[Test]
		public void VerifyMultiplicatorDefinitionSetsCanBeRead()
		{
			mocks.BackToRecord(_schedulingResultLoader);

			var multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
			Expect.Call(_schedulingResultLoader.MultiplicatorDefinitionSets).Return(multiplicatorDefinitionSets);

			mocks.Replay(_schedulingResultLoader);

			Assert.AreEqual(multiplicatorDefinitionSets,_target.MultiplicatorDefinitionSets);

			mocks.Verify(_schedulingResultLoader);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyPrepareSkillIntradayCollection()
		{
			var period = _schedulerStateHolder.RequestedPeriod.Period();
			var smallPeriod = new DateTimePeriod(period.StartDateTime,period.StartDateTime.AddMinutes(15));
            var skill = mocks.StrictMock<ISkill>();
            var skillDay = mocks.DynamicMock<ISkillDay>();
            
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod, new Task(),
																	   ServiceAgreement.DefaultValues());
			ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod.MovePeriod(TimeSpan.FromMinutes(15)), new Task(),
																	   ServiceAgreement.DefaultValues());
			ISkillStaffPeriod skillStaffPeriod3 =
				SkillStaffPeriodFactory.CreateSkillStaffPeriod(smallPeriod.MovePeriod(TimeSpan.FromMinutes(15).Add(TimeSpan.FromDays(1))),
									 new Task(), ServiceAgreement.DefaultValues());

			skillStaffPeriod1.SetParent(skillDay);
			skillStaffPeriod2.SetParent(skillDay);
			skillStaffPeriod3.SetParent(skillDay);

			Expect.Call(skill.Name).Return("SkillTest").Repeat.AtLeastOnce();
			Expect.Call(skillDay.SkillStaffPeriodCollection).Return(
				new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>
															  {skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3}));
            
			_schedulerStateHolder.SchedulingResultState.Skills.Add(skill);
			_schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			_schedulerStateHolder.SchedulingResultState.SkillDays.Add(skill, new List<ISkillDay> { skillDay });

			Expect.Call(_view.SelectedSkill).Return(skill);

			mocks.BackToRecord(_schedulingResultLoader);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

			mocks.ReplayAll();
			IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
			mocks.VerifyAll();
			Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "SkillTest - {0}",_target.IntradayDate.ToShortDateString()),_target.ChartIntradayDescription);
			Assert.AreEqual(2,skillStaffPeriods.Count);
		}

		[Test]
		public void VerifyPrepareSkillIntradayCollectionWithNotContainedSkill()
		{
			ISkill skill = mocks.StrictMock<ISkill>();
			_schedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			Expect.Call(_view.SelectedSkill).Return(skill);

			mocks.BackToRecord(_schedulingResultLoader);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder);

			mocks.ReplayAll();
			IList<ISkillStaffPeriod> skillStaffPeriods = _target.PrepareSkillIntradayCollection();
			mocks.VerifyAll();
			Assert.AreEqual(0, skillStaffPeriods.Count);
		}

		[Test]
		public void VerifyOnEventStatisticMessageHandler()
		{
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			using (mocks.Record())
			{
				Expect.Call(_view.InvokeRequired).Return(false);
				Expect.Call(eventMessage.ModuleId).Return(Guid.Empty);
				Expect.Call(()=>_statisticCommand.Execute(eventMessage));
			}
			
			using (mocks.Playback())
			{
				_target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
			}
		}

		[Test]
		public void VerifyOnEventForecastDataMessageHandlerInvokeRequired()
		{
			IAsyncResult result = mocks.StrictMock<IAsyncResult>();
			Expect.Call(_view.InvokeRequired).Return(true);
			Expect.Call(_view.BeginInvoke(new Action<object,EventMessageArgs>(_target.OnEventForecastDataMessageHandler), new object[]{})).IgnoreArguments().Return(result);
			mocks.ReplayAll();
			_target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(new EventMessage()));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventScheduleMessageHandlerInvokeRequired()
		{
			IAsyncResult result = mocks.StrictMock<IAsyncResult>();
			Expect.Call(_view.InvokeRequired).Return(true);
			Expect.Call(_view.BeginInvoke(new Action<object,EventMessageArgs>(_target.OnEventScheduleMessageHandler), new object(),new object())).IgnoreArguments().Return(result);
			mocks.ReplayAll();
			_target.OnEventScheduleMessageHandler(null, new EventMessageArgs(new EventMessage()));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventStatisticMessageHandlerInvokeRequired()
		{
			IAsyncResult result = mocks.StrictMock<IAsyncResult>();
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			Expect.Call(_view.InvokeRequired).Return(true);
			Expect.Call(_view.BeginInvoke(new EventHandler<EventMessageArgs>(_target.OnEventStatisticMessageHandler), new object[] { })).IgnoreArguments().Return(result);
			mocks.ReplayAll();
			_target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventForecastDataMessageHandlerSameModuleId()
		{
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
			mocks.ReplayAll();
			_target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(eventMessage));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventScheduleMessageHandlerSameModuleId()
		{
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
			mocks.ReplayAll();
			_target.OnEventScheduleMessageHandler(null, new EventMessageArgs(eventMessage));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventExternalAgentStateMessageHandlerSameModuleId()
		{
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
			mocks.ReplayAll();
			_target.OnEventExternalAgentStateMessageHandler(null, new EventMessageArgs(eventMessage));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventStatisticMessageHandlerSameModuleId()
		{
			IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
			Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(eventMessage.ModuleId).Return(_target.ModuleId);
			mocks.ReplayAll();
			_target.OnEventStatisticMessageHandler(null, new EventMessageArgs(eventMessage));
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventForecastDataMessageHandler()
		{
			var message = new EventMessage();

            Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(()=>_forecastCommand.Execute(message));

			mocks.ReplayAll();

			_target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(message));

			mocks.VerifyAll();
		}

		private void createRealTimeAdherenceInitializeExpectation()
		{
			IStatisticRepository statisticRepository = mocks.StrictMock<IStatisticRepository>();

			Expect.Call(_repositoryFactory.CreateStatisticRepository()).Return(statisticRepository).Repeat.Any();

            Expect.Call(statisticRepository.LoadRtaAgentStates(new DateTimePeriod(), new List<ExternalLogOnPerson>())).IgnoreArguments().Return(
				new List<IExternalAgentState>()).Repeat.Any();
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerAssignment()
		{
			var message = new EventMessage
			              	{
			              		InterfaceType = typeof (IPersonAssignment),
			              		DomainObjectId = Guid.NewGuid(),
			              		DomainUpdateType = DomainUpdateType.Insert
			              	};

			Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(() => _scheduleCommand.Execute(message));

			mocks.ReplayAll();

			_target.OnEventScheduleMessageHandler(null, new EventMessageArgs(message));

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnEventScheduleDataMessageHandlerDeleteAssignment()
		{
			var idFromBroker = Guid.NewGuid();
			var message = new EventMessage
			              	{
			              		InterfaceType = typeof (IPersonAssignment),
			              		DomainObjectId = idFromBroker,
			              		DomainUpdateType = DomainUpdateType.Delete
			              	};

			Expect.Call(_view.InvokeRequired).Return(false);
			Expect.Call(()=>_scheduleCommand.Execute(message));

			mocks.ReplayAll();

			_target.OnEventScheduleMessageHandler(null, new EventMessageArgs(message));

			mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[Test]
		public void VerifyOnEventExternalAgentStateMessageHandler()
		{
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var eventMessage = mocks.StrictMock<IEventMessage>();
            var authorization = mocks.StrictMock<IPrincipalAuthorization>();
			var stateHolder = new SchedulerStateHolder(_scenario,
			                                           new DateOnlyPeriodAsDateTimePeriod(_periodNow,
			                                                                              TeleoptiPrincipal.Current.Regional.
			                                                                              	TimeZone), _persons);
			var period = stateHolder.RequestedPeriod.Period();
			
            IExternalAgentState externalAgentState = new ExternalAgentState("001", "AUX2", TimeSpan.Zero, DateTime.MinValue, Guid.NewGuid(), 1, DateTime.UtcNow, false);
            byte[] data = new ExternalAgentStateEncoder().Encode(externalAgentState);

            mocks.BackToRecord(_schedulingResultLoader);

			createRealTimeAdherenceInitializeExpectation();
            createRtaStateHolderExpectation();

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
			
			Expect.Call(()=>_messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null)).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
																	period.EndDateTime)).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, Guid.Empty, typeof(IExternalAgentState), period.StartDateTime,
                                                                    period.EndDateTime)).IgnoreArguments().Repeat.Times(1);
			Expect.Call(eventMessage.ModuleId).Return(Guid.Empty);
            Expect.Call(eventMessage.DomainObject).Return(data);

		    Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning)).Return(true);
			Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)).Return(true);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(stateHolder).Repeat.AtLeastOnce();
			
			Expect.Call(()=>_schedulingResultLoader.LoadWithIntradayData(uow));

			mocks.ReplayAll();
            using (new CustomAuthorizationContext(authorization))
            {
                _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder,
                                                _eventAggregator, null, null, _unitOfWorkFactory, _repositoryFactory,
                                                _statisticCommand, _forecastCommand, _scheduleCommand, _loadStatisticCommand);
                _target.Initialize();
                bool updateMessageReceived = false;
                _target.ExternalAgentStateReceived += (x, y) =>
                                                          {
                                                              updateMessageReceived = true;
                                                          };
                _target.OnEventExternalAgentStateMessageHandler(null, new EventMessageArgs(eventMessage));

                Assert.That(() => updateMessageReceived, Is.True.After(10000, 10));
            }
		    mocks.VerifyAll();
		}

	    private void createRtaStateHolderExpectation()
	    {
	        var emptyLogOnList = new ExternalLogOnPerson[] {};
	        Expect.Call(_rtaStateHolder.ExternalLogOnPersons).Return(emptyLogOnList);
	        Expect.Call(() => _rtaStateHolder.CollectAgentStates(null)).IgnoreArguments().Repeat.AtLeastOnce();
	        Expect.Call(()=>_rtaStateHolder.AnalyzeAlarmSituations(DateTime.UtcNow)).IgnoreArguments();
	        Expect.Call(_rtaStateHolder.Initialize);
	        Expect.Call(_rtaStateHolder.InitializeSchedules);
	        Expect.Call(()=>_rtaStateHolder.SetFilteredPersons(_schedulerStateHolder.FilteredPersonDictionary.Values));
	        Expect.Call(_rtaStateHolder.VerifyDefaultStateGroupExists);
	        Expect.Call(() => _rtaStateHolder.RtaStateCreated += (sender,e) => {}).IgnoreArguments();
	    }

	    [Test]
		public void VerifyOnLoad()
		{
			mocks.BackToRecord(_schedulingResultLoader);
            
			var uow = mocks.DynamicMock<IUnitOfWork>();
	    	var period = _schedulerStateHolder.RequestedPeriod.Period();
			
			createRealTimeAdherenceInitializeExpectation();
            createRtaStateHolderExpectation();
            
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();

			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null)).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
																	period.EndDateTime)).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, Guid.Empty, null, period.StartDateTime,
																	period.EndDateTime)).IgnoreArguments().Repeat.Times(2);
			
			Expect.Call(()=>_schedulingResultLoader.LoadWithIntradayData(uow));
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
			
			mocks.ReplayAll();

            _schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2),DateOnly.Today.AddDays(2)), TimeZoneInfo.Utc);
			_schedulerStateHolder.FilteredPersonDictionary.Add(Guid.NewGuid(), _persons[0]);
            _target.Initialize();
			
			mocks.VerifyAll();

			Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
		}

        [Test]
        public void VerifyOnLoadWithMoreThanOneHundredPeople()
        {
            mocks.BackToRecord(_schedulingResultLoader);

            var uow = mocks.DynamicMock<IUnitOfWork>();
            var period = _schedulerStateHolder.RequestedPeriod.Period();

            createRealTimeAdherenceInitializeExpectation();
            createRtaStateHolderExpectation();

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();

            Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null)).IgnoreArguments().Repeat.Times(2);
            Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
                                                                    period.EndDateTime)).IgnoreArguments().Repeat.Times(3);

            Expect.Call(() => _schedulingResultLoader.LoadWithIntradayData(uow));
            Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

            mocks.ReplayAll();

            _schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-2), DateOnly.Today.AddDays(2)), TimeZoneInfo.Utc);
            Enumerable.Range(0, 101)
                      .ForEach(_ => _schedulerStateHolder.FilteredPersonDictionary.Add(Guid.NewGuid(), _persons[0]));
            _target.Initialize();

            mocks.VerifyAll();

            Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
        }

		[Test]
		public void VerifyOnLoadWithoutRtaEnabled()
		{
			mocks.BackToRecord(_schedulingResultLoader);

			var uow = mocks.DynamicMock<IUnitOfWork>();
            var authorization = mocks.StrictMock<IPrincipalAuthorization>();
			var period = _schedulerStateHolder.RequestedPeriod.Period();

            createRealTimeAdherenceInitializeExpectation();
            
            Expect.Call(_rtaStateHolder.Initialize);
            Expect.Call(_rtaStateHolder.InitializeSchedules);
            Expect.Call(() => _rtaStateHolder.SetFilteredPersons(_schedulerStateHolder.FilteredPersonDictionary.Values));
	        
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
			
			Expect.Call(()=>_schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();

			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null)).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
																	period.EndDateTime)).IgnoreArguments().Repeat.Times(2);
			
			Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning)).Return(true);
			Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence)).Return(false);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
			
			mocks.ReplayAll();
            using (new CustomAuthorizationContext(authorization))
            {
                _target = new IntradayPresenter(_view, _schedulingResultLoader, _messageBroker, _rtaStateHolder,
                                                _eventAggregator, null, null, _unitOfWorkFactory, _repositoryFactory,
                                                _statisticCommand, _forecastCommand, _scheduleCommand, _loadStatisticCommand);
                _target.Initialize();
            }
		    mocks.VerifyAll();

			Assert.AreEqual(_rtaStateHolder, _target.RtaStateHolder);
		}

		[Test]
		public void VerifyHandlesNewStateAdded()
		{
			IUnitOfWork uow = mocks.StrictMock<IUnitOfWork>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(uow.PersistAll(_target)).Return(new List<IRootChangeInfo>());
			uow.Dispose();
			
			IRtaState theState = mocks.StrictMock<IRtaState>();
			IRtaStateGroup theStateGroup = mocks.StrictMock<IRtaStateGroup>();
			IRtaStateGroupRepository rtaStateGroupRepository = mocks.StrictMock<IRtaStateGroupRepository>();
			Expect.Call(_repositoryFactory.CreateRtaStateGroupRepository(uow)).Return(rtaStateGroupRepository);
			Expect.Call(theState.StateGroup).Return(theStateGroup);
			rtaStateGroupRepository.Add(theStateGroup);

			mocks.ReplayAll();

			_target.GetType().GetMethod("_rtaStateHolder_RtaStateCreated",
										BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_target,
																							   new object[]
																								   {
																									   null,
																									   new CustomEventArgs
																										   <IRtaState>(
																										   theState)
																								   });

			mocks.VerifyAll();

			Assert.IsNotNull(_target.RtaStateHolder);
		}

		[Test]
		public void VerifyHandlesNewStateAddedOnTwoClientsWithoutException()
		{
			IUnitOfWork uow = mocks.StrictMock<IUnitOfWork>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(uow.PersistAll(_target)).Throw(new OptimisticLockException());
			uow.Dispose();

			IRtaState theState = mocks.StrictMock<IRtaState>();
			IRtaStateGroup theStateGroup = mocks.StrictMock<IRtaStateGroup>();
			IRtaStateGroupRepository rtaStateGroupRepository = mocks.StrictMock<IRtaStateGroupRepository>();
			Expect.Call(_repositoryFactory.CreateRtaStateGroupRepository(uow)).Return(rtaStateGroupRepository);
			Expect.Call(theState.StateGroup).Return(theStateGroup);
			rtaStateGroupRepository.Add(theStateGroup);
			mocks.ReplayAll();

			_target.GetType().GetMethod("_rtaStateHolder_RtaStateCreated",
										BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_target,
																							   new object[]
																								   {
																									   null,
																									   new CustomEventArgs
																										   <IRtaState>(
																										   theState)
																								   });

			mocks.VerifyAll();

			Assert.IsNotNull(_target.RtaStateHolder);
		}

		[Test]
		public void VerifyOnLoadWithPersons()
		{
			mocks.BackToRecord(_schedulingResultLoader);

			var uow = mocks.DynamicMock<IUnitOfWork>();
			var period = _schedulerStateHolder.RequestedPeriod.Period();

            createRealTimeAdherenceInitializeExpectation();
            createRtaStateHolderExpectation();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce(); 
			
			Expect.Call(()=>_schedulingResultLoader.LoadWithIntradayData(uow)).Repeat.AtLeastOnce();

			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, typeof(IStatisticTask))).IgnoreArguments().Repeat.Times(2);
			Expect.Call(() => _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, null, period.StartDateTime,
																	period.EndDateTime)).IgnoreArguments().Repeat.Times(2);

			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();
			
			mocks.ReplayAll();

			_target.Initialize();

			mocks.VerifyAll();

			Assert.IsTrue(_schedulerStateHolder.AllPermittedPersons.Contains(_persons[0]));
		}

		[Test]
		public void VerifySave()
		{
			IUnitOfWork uow =mocks.StrictMock<IUnitOfWork>();
			IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			mocks.BackToRecord(_schedulingResultLoader);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.Any();
			
			using (mocks.Ordered())
			{
				Expect.Call(_view.UpdateFromEditor);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

				CreateBasicExpectationForSave(uow, scheduleDictionary);
				
                Expect.Call(uow.PersistAll(_target)).Return(new List<IRootChangeInfo>());
				Expect.Call(uow.Dispose);
				Expect.Call(()=>_view.UpdateShiftEditor(new List<IScheduleDay>()));
				Expect.Call(_view.DisableSave);
				Expect.Call(_view.UpdateFromEditor);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				CreateBasicExpectationForSave(uow, scheduleDictionary);
				Expect.Call(uow.PersistAll(_target)).Throw(new OptimisticLockException());
				Expect.Call(uow.Dispose);
				Expect.Call(()=>_view.ShowInformationMessage("","")).IgnoreArguments();
				
				Expect.Call(()=>_view.ToggleSchedulePartModified(false));

                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(()=>_schedulingResultLoader.ReloadScheduleData(uow));
                Expect.Call(uow.Dispose);
                Expect.Call(_rtaStateHolder.InitializeSchedules);
				Expect.Call(_view.RefreshRealTimeScheduleControls);
				Expect.Call(()=>_view.ToggleSchedulePartModified(true));

				Expect.Call(_view.DrawSkillGrid);
				Expect.Call(()=>_view.UpdateShiftEditor(new List<IScheduleDay>()));
				Expect.Call(_view.DisableSave);
			}

			mocks.ReplayAll();
			_target.Save();
			_target.Save();

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyCheckIfUserWantsToSaveUnsavedData()
		{
			IUnitOfWork uow = mocks.StrictMock<IUnitOfWork>();
			IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			var differenceCollection = new DifferenceCollection<IPersistableScheduleData>();
			
			mocks.BackToRecord(_schedulingResultLoader);
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.Any();
			using (mocks.Ordered())
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
				Expect.Call(()=>_view.UpdateShiftEditor(new List<IScheduleDay>()));
				Expect.Call(_view.DisableSave);
			}

			mocks.ReplayAll();
			Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
			differenceCollection.Add(new DifferenceCollectionItem<IPersistableScheduleData>());
			Assert.IsTrue(_target.CheckIfUserWantsToSaveUnsavedData());
			Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());
			Assert.IsFalse(_target.CheckIfUserWantsToSaveUnsavedData());

			mocks.VerifyAll();
		}

		private void CreateBasicExpectationForSave(IUnitOfWork uow, IScheduleDictionary scheduleDictionary)
		{
			var changes = new DifferenceCollection<IPersistableScheduleData>();

			Expect.Call(()=>uow.Reassociate(_schedulerStateHolder.CommonStateHolder.Activities));
			Expect.Call(()=>uow.Reassociate(_schedulerStateHolder.CommonStateHolder.Absences));
			Expect.Call(()=>uow.Reassociate(_schedulerStateHolder.CommonStateHolder.ShiftCategories));

            Expect.Call(() => uow.Reassociate(new List<IPerson>()));

			var multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
			Expect.Call(_schedulingResultLoader.MultiplicatorDefinitionSets).Return(multiplicatorDefinitionSets);
			Expect.Call(()=>uow.Reassociate(multiplicatorDefinitionSets));

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
			Assert.AreEqual(_period.StartDate,_target.IntradayDate);
			Assert.IsNotNull(_target.RtaStateHolder);
			Assert.AreNotEqual(Guid.Empty,_target.ModuleId);
			Assert.IsTrue(_target.RealTimeAdherenceEnabled);
			Assert.IsTrue(_target.EarlyWarningEnabled);
		}

		[Test]
		public void VerifyCanSetIntradayDate()
		{
			using (mocks.Record())
			{
				_view.DrawSkillGrid();
				LastCall.Repeat.Once();
			}
			using (mocks.Playback())
			{
				_target.IntradayDate = _period.EndDate;
			}
			Assert.AreEqual(_period.EndDate,_target.IntradayDate);
		}

		[Test]
		public void VerifyPrepareChartDescription()
		{
			string description = IntradayPresenter.PrepareChartDescription("{0} - {1}", "test1", "test2");
			Assert.AreEqual("test1 - test2", description);
		}

	    [Test]
		public void VerifyRefreshDataWithoutReceivedEvent()
		{
			DateTime timestamp = DateTime.UtcNow;
			TimeSpan refreshRate = TimeSpan.FromSeconds(2);

			_rtaStateHolder.UpdateCurrentLayers(timestamp,refreshRate);
			_rtaStateHolder.AnalyzeAlarmSituations(timestamp);

			mocks.ReplayAll();
			_target.RefreshAgentStates(timestamp,refreshRate);
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyCanGetAgentStateViewAdapterCollection()
		{
			var stategroups = new ReadOnlyCollection<IRtaStateGroup>(new List<IRtaStateGroup> {new RtaStateGroup("test",true,true)});
			Expect.Call(_rtaStateHolder.RtaStateGroups).Return(stategroups);
			mocks.ReplayAll();
			string test = "";
			foreach (var adaper in _target.CreateAgentStateViewAdapterCollection())
			{
				test = adaper.StateGroup.Name;         
			}
			mocks.VerifyAll();
			Assert.AreEqual( "test",test);
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
			mocks.BackToRecord(_messageBroker);
			mocks.BackToRecord(_rtaStateHolder);
			_messageBroker.UnregisterEventSubscription(_target.OnEventForecastDataMessageHandler);
			_messageBroker.UnregisterEventSubscription(_target.OnEventScheduleMessageHandler);
			_messageBroker.UnregisterEventSubscription(_target.OnEventStatisticMessageHandler);
			_messageBroker.UnregisterEventSubscription(_target.OnEventExternalAgentStateMessageHandler);
			_rtaStateHolder.RtaStateCreated -= null;
			LastCall.IgnoreArguments().Repeat.Any();
			mocks.Replay(_rtaStateHolder);
			mocks.Replay(_messageBroker);
			_target.Dispose();
			mocks.Verify(_messageBroker);
			mocks.Verify(_rtaStateHolder);
		}
	}
}
