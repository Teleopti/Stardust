using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using log4net;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class IntradayPresenter : IInitiatorIdentifier, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IntradayPresenter));

        private IIntradayView _view;
        private ISchedulingResultLoader _schedulingResultLoader;
        private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
	    private IMessageBroker _messageBroker;
        private string _chartIntradayDescription = string.Empty;
        private DateOnly _intradayDate;
        private IRtaStateHolder _rtaStateHolder;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly bool _realTimeAdherenceEnabled;
        private readonly bool _earlyWarningEnabled;
        private readonly IEventAggregator _eventAggregator;
				private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
        private readonly OnEventStatisticMessageCommand _onEventStatisticMessageCommand;
        private readonly OnEventForecastDataMessageCommand _onEventForecastDataMessageCommand;
        private readonly OnEventScheduleMessageCommand _onEventScheduleMessageCommand;
        private readonly OnEventMeetingMessageCommand _onEventMeetingMessageCommand;
        private readonly LoadStatisticsAndActualHeadsCommand _loadStatisticsAndActualHeadsCommand;
        private readonly Queue<MessageForRetryCommand> _messageForRetryQueue = new Queue<MessageForRetryCommand>();

        public IntradayPresenter(IIntradayView view,
            ISchedulingResultLoader schedulingResultLoader,
            IMessageBroker messageBroker,
            IRtaStateHolder rtaStateHolder,
            IEventAggregator eventAggregator,
						IScheduleDifferenceSaver scheduleDictionarySaver,
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
					IDifferenceCollectionService<IPersistableScheduleData> differenceService,
					OnEventStatisticMessageCommand onEventStatisticMessageCommand,
            OnEventForecastDataMessageCommand onEventForecastDataMessageCommand,
            OnEventScheduleMessageCommand onEventScheduleMessageCommand,
            OnEventMeetingMessageCommand onEventMeetingMessageCommand,
            LoadStatisticsAndActualHeadsCommand loadStatisticsAndActualHeadsCommand)
        {
            _eventAggregator = eventAggregator;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _onEventStatisticMessageCommand = onEventStatisticMessageCommand;
            _onEventForecastDataMessageCommand = onEventForecastDataMessageCommand;
            _onEventScheduleMessageCommand = onEventScheduleMessageCommand;
            _onEventMeetingMessageCommand = onEventMeetingMessageCommand;
            _loadStatisticsAndActualHeadsCommand = loadStatisticsAndActualHeadsCommand;
            _repositoryFactory = repositoryFactory;
	        _differenceService = differenceService;
	        _messageBroker = messageBroker;
            _rtaStateHolder = rtaStateHolder;
            _unitOfWorkFactory = unitOfWorkFactory;
            _view = view;
            _earlyWarningEnabled =
                PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning);
            _realTimeAdherenceEnabled =
                PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence);

            _schedulingResultLoader = schedulingResultLoader;

            _intradayDate = HistoryOnly ? SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate : DateOnly.Today;
        }

        public bool EarlyWarningEnabled
        {
            get { return _earlyWarningEnabled; }
        }

        public bool RealTimeAdherenceEnabled
        {
            get { return _realTimeAdherenceEnabled; }
        }

        public string ChartIntradayDescription
        {
            get { return _chartIntradayDescription; }
        }

        private void listenForMessageBroker()
        {
            if (_messageBroker == null) return;

            var period = SchedulerStateHolder.RequestedPeriod.Period();
            _messageBroker.RegisterEventSubscription(OnEventStatisticMessageHandler,
                                                    typeof(IStatisticTask));
            _messageBroker.RegisterEventSubscription(OnEventScheduleMessageHandler,
                                                    SchedulerStateHolder.RequestedScenario.Id.GetValueOrDefault(),
                                                    typeof(Scenario),
                                                    typeof(IScheduleChangedEvent),
                                                    period.StartDateTime,
                                                    period.EndDateTime);
        	_messageBroker.RegisterEventSubscription(OnEventMeetingMessageHandler,
        	                                         typeof (IMeetingChangedEntity));
            _messageBroker.RegisterEventSubscription(OnEventForecastDataMessageHandler,
                                                    typeof(IForecastData),
                                                    period.StartDateTime,
                                                    period.EndDateTime);

            if (!_realTimeAdherenceEnabled || HistoryOnly) return;

            if (SchedulerStateHolder.FilteredPersonDictionary.Count>100)
            {
                    _messageBroker.RegisterEventSubscription(OnEventActualAgentStateMessageHandler,
                                                            typeof(IActualAgentState),
                                                            DateTime.UtcNow,
                                                            DateTime.UtcNow.AddDays(1));
            }
            else
            {
                foreach (var person in SchedulerStateHolder.FilteredPersonDictionary.Values)
                {
                    _messageBroker.RegisterEventSubscription(OnEventActualAgentStateMessageHandler,
                                                            person.Id.GetValueOrDefault(),
                                                            typeof(IActualAgentState),
                                                            DateTime.UtcNow,
                                                            DateTime.UtcNow.AddDays(1));
                }
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void OnEventActualAgentStateMessageHandler(object sender, EventMessageArgs e)
        {
            if (e.Message.ModuleId == _instanceId) return;

            ThreadPool.QueueUserWorkItem(handleIncomingExternalEvent, e.Message.DomainObject);
        }

        private void handleIncomingExternalEvent(object state)
        {
            var stateBytes = state as byte[];
            if (stateBytes == null)
                return;
            IActualAgentState agentState = JsonConvert.DeserializeObject<ActualAgentState>(Encoding.UTF8.GetString(stateBytes));

            Logger.DebugFormat("Externalstate received: State {0}, PersonId {1}, State start {2}", agentState.State, agentState.PersonId, agentState.StateStart);
            _rtaStateHolder.SetActualAgentState(agentState);
            if (ExternalAgentStateReceived != null) ExternalAgentStateReceived.Invoke(this, EventArgs.Empty);
        }

        public ISchedulerStateHolder SchedulerStateHolder
        {
            get { return _schedulingResultLoader.SchedulerState; }
        }

        public IEnumerable<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets
        {
            get { return _schedulingResultLoader.MultiplicatorDefinitionSets; }
        }

        public IRtaStateHolder RtaStateHolder
        {
            get { return _rtaStateHolder; }
        }

        public DateOnly IntradayDate
        {
            get { return _intradayDate; }
            set
            {
                _intradayDate = value;
                _view.DrawSkillGrid();
            }
        }

        public void OnEventForecastDataMessageHandler(object sender, EventMessageArgs e)
        {
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new Action<object, EventMessageArgs>(OnEventForecastDataMessageHandler), sender, e);
            }
            else
            {
                if (e.Message.ModuleId == _instanceId) return;

                try
                {
                    _onEventForecastDataMessageCommand.Execute(e.Message);
                }
                catch (DataSourceException)
                {
                    _messageForRetryQueue.Enqueue(new MessageForRetryCommand(_onEventForecastDataMessageCommand, e.Message));
                    _view.ShowBackgroundDataSourceError();
                }
            }
        }

        public void OnEventScheduleMessageHandler(object sender, EventMessageArgs e)
        {
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new Action<object, EventMessageArgs>(OnEventScheduleMessageHandler), sender, e);
            }
            else
            {
                if (e.Message.ModuleId == _instanceId) return;

                try
                {
                    _onEventScheduleMessageCommand.Execute(e.Message);
                }
                catch (DataSourceException)
                {
                    _messageForRetryQueue.Enqueue(new MessageForRetryCommand(_onEventScheduleMessageCommand,
                                                                             e.Message));
                    _view.ShowBackgroundDataSourceError();
                }
            }
        }

        public void OnEventMeetingMessageHandler(object sender, EventMessageArgs e)
        {
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new Action<object, EventMessageArgs>(OnEventMeetingMessageHandler), sender, e);
            }
            else
            {
                if (e.Message.ModuleId == _instanceId) return;

                try
                {
                    _onEventMeetingMessageCommand.Execute(e.Message);
                }
                catch (DataSourceException)
                {
                    _messageForRetryQueue.Enqueue(new MessageForRetryCommand(_onEventMeetingMessageCommand,
                                                                             e.Message));
                    _view.ShowBackgroundDataSourceError();
                }
            }
        }

        private void reassociateCommonStateHolder(IUnitOfWork uow)
        {
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Activities);
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.Absences);
            uow.Reassociate(_schedulingResultLoader.SchedulerState.CommonStateHolder.ShiftCategories);
        }

        public void OnEventStatisticMessageHandler(object sender, EventMessageArgs e)
        {
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new Action<object, EventMessageArgs>(OnEventStatisticMessageHandler), sender, e);
            }
            else
            {
                if (e.Message.ModuleId == _instanceId) return;

                try
                {
                    _onEventStatisticMessageCommand.Execute(e.Message);
                }
                catch (DataSourceException)
                {
                    _messageForRetryQueue.Enqueue(new MessageForRetryCommand(_onEventStatisticMessageCommand,
                                                                             e.Message));
                    _view.ShowBackgroundDataSourceError();
                }
            }
        }

        public void Initialize()
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _schedulingResultLoader.LoadWithIntradayData(uow);
                initializeRtaStateHolder();
            }
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.RegisteringWithMessageBrokerThreeDots);
            listenForMessageBroker();
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingInitialStatesThreeDots);
            loadExternalAgentStates();
        }

        private void loadExternalAgentStates()
        {
            if (!_realTimeAdherenceEnabled) return; //If RTA isn't enabled, we don't need the rest of the stuff...

            var statisticRepository = _repositoryFactory.CreateStatisticRepository();
            using (PerformanceOutput.ForOperation("Read and collect agent states"))
            {
                var tmp = statisticRepository.LoadActualAgentState(_rtaStateHolder.FilteredPersons);
                foreach (var actualAgentState in tmp)
                {
					// if we have recieved an RTA event that is more recent than what we get from DB
	                IActualAgentState outState;
	                if (_rtaStateHolder.ActualAgentStates.TryGetValue(actualAgentState.PersonId, out outState))
		                _rtaStateHolder.SetActualAgentState(outState.ReceivedTime > actualAgentState.ReceivedTime
			                                                    ? outState
			                                                    : actualAgentState);
	                else
		                _rtaStateHolder.SetActualAgentState(actualAgentState);
                }
            }
        }

        private void initializeRtaStateHolder()
        {
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingRealTimeAdherenceDataThreeDots);

            _rtaStateHolder.Initialize();
            _rtaStateHolder.SetFilteredPersons(SchedulerStateHolder.FilteredPersonDictionary.Values);

            if (!_realTimeAdherenceEnabled) return;

            _rtaStateHolder.VerifyDefaultStateGroupExists();
        }

        public static string PrepareChartDescription(string format, params string[] descriptions)
        {
            return string.Format(CultureInfo.CurrentCulture, format, descriptions);
        }

        public IList<ISkillStaffPeriod> PrepareSkillIntradayCollection()
        {
            ISkill skill = _view.SelectedSkill;
            DateTimePeriod utcDayPeriod =
                new DateOnlyPeriod(_intradayDate, _intradayDate).ToDateTimePeriod(TimeZoneHelper.CurrentSessionTimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                SchedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
                    new List<ISkill> { skill }, utcDayPeriod);
            if (skillStaffPeriods.Count == 0) return skillStaffPeriods;

            MultisiteSkillDayCalculator.SummarizeStaffingForMultisiteSkillDays(skill as IMultisiteSkill,
                                                                               SchedulerStateHolder.
                                                                                   SchedulingResultState.SkillDays);

            _chartIntradayDescription = PrepareChartDescription("{0} - {1}", skill.Name,
                                                                _intradayDate.ToShortDateString());

            _loadStatisticsAndActualHeadsCommand.Execute(_intradayDate, skill, utcDayPeriod, skillStaffPeriods);

            return skillStaffPeriods;
        }

        public void UnregisterMessageBrokerEvents()
        {
            if (_messageBroker == null) return;
            _messageBroker.UnregisterEventSubscription(OnEventActualAgentStateMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventForecastDataMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventScheduleMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventStatisticMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventMeetingMessageHandler);
        }

        public void Save()
        {
            _view.UpdateFromEditor();

            try
            {
                using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    reassociateCommonStateHolder(uow);
                    uow.Reassociate(_schedulingResultLoader.SchedulerState.SchedulingResultState.PersonsInOrganization);
                    uow.Reassociate(MultiplicatorDefinitionSets);
	                foreach (var range in SchedulerStateHolder.Schedules.Values)
	                {
		                _scheduleDictionarySaver.SaveChanges(range.DifferenceSinceSnapshot(_differenceService), (IUnvalidatedScheduleRangeUpdate) range);
	                }
                    uow.PersistAll(this);
                }
            }
            catch (OptimisticLockException)
            {
                _view.ShowInformationMessage(string.Concat(UserTexts.Resources.SomeoneElseHaveChanged," ",
                         UserTexts.Resources.YourChangesWillBeDiscardedReloading), "  ");

                _view.ToggleSchedulePartModified(false);
                using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    _schedulingResultLoader.ReloadScheduleData(unitOfWork);
                }
                _view.RefreshRealTimeScheduleControls();
                _view.ToggleSchedulePartModified(true);
                _view.DrawSkillGrid();
            }
            _view.UpdateShiftEditor(new List<IScheduleDay>());
            _view.DisableSave();
        }

        public bool CheckIfUserWantsToSaveUnsavedData()
        {
            if (SchedulerStateHolder.Schedules != null &&
                !SchedulerStateHolder.Schedules.DifferenceSinceSnapshot().IsEmpty())
            {
                var res = _view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
                                                           UserTexts.Resources.Save);
                switch (res)
                {
                    case DialogResult.Cancel:
                        return true;
                    case DialogResult.No:
                        return false;
                    case DialogResult.Yes:
                        Save();
                        return false;
                }
            }

            return false;
        }

        public Guid InitiatorId
        {
            get { return _instanceId; }
        }

        public bool HistoryOnly
        {
            get { return !SchedulerStateHolder.RequestedPeriod.Period().Contains(DateTime.UtcNow); }
        }

		public IScenario RequestedScenario
		{
			get { return SchedulerStateHolder.RequestedScenario; }
		}

        public event EventHandler ExternalAgentStateReceived;
		
        public IDayLayerViewModel CreateDayLayerViewModel()
        {
            var viewModel = new DayLayerViewModel(_rtaStateHolder, _eventAggregator, _unitOfWorkFactory, _repositoryFactory, new DispatcherWrapper());
            viewModel.CreateModels(SchedulerStateHolder.FilteredPersonDictionary.Values,
                                   SchedulerStateHolder.RequestedPeriod);
            return viewModel;
        }

        public IEnumerable<AgentStateViewAdapter> CreateAgentStateViewAdapterCollection(IDayLayerViewModel model)
        {
            return _rtaStateHolder.RtaStateGroups.Select(rtaStateGroup => new AgentStateViewAdapter(rtaStateGroup, model));
        }

        public void RetryHandlingMessages()
        {
            while (_messageForRetryQueue.Count > 0)
            {
                var handler = _messageForRetryQueue.Peek();
                try
                {
                    handler.Execute();
                }
                catch (DataSourceException dataSourceException)
                {
                    Logger.Error("Something wrong with the data source.", dataSourceException);
                    _view.ShowBackgroundDataSourceError();
                    return;
                }
                catch (Exception exception)
                {
                    Logger.Error("An error occurred!", exception);
                    throw;
                }
                _messageForRetryQueue.Dequeue();
            }
            _view.HideBackgroundDataSourceError();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void ReleaseManagedResources()
        {
            UnregisterMessageBrokerEvents();
            _messageForRetryQueue.Clear();
            _messageBroker = null;
            _view = null;
            _rtaStateHolder = null;
            _schedulingResultLoader = null;
            _unitOfWorkFactory = null;
        }

        private class MessageForRetryCommand
        {
            private readonly IMessageHandlerCommand _messageHandlerCommand;
            private readonly IEventMessage _message;

            public MessageForRetryCommand(IMessageHandlerCommand messageHandlerCommand, IEventMessage message)
            {
                _messageHandlerCommand = messageHandlerCommand;
                _message = message;
            }

            public void Execute()
            {
                _messageHandlerCommand.Execute(_message);
            }
        }
    }
}