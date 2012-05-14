﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using log4net;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using System.Windows.Forms;
using Teleopti.Messaging.Coders;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class IntradayPresenter : IMessageBrokerModule, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IntradayPresenter));

        private IIntradayView _view;
        private ISchedulingResultLoader _schedulingResultLoader;
        private readonly IRepositoryFactory _repositoryFactory;
        private IMessageBroker _messageBroker;
        private string _chartIntradayDescription = string.Empty;
        private DateOnly _intradayDate;
        private IRtaStateHolder _rtaStateHolder;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly Guid _moduleId = Guid.NewGuid();
        private readonly bool _realTimeAdherenceEnabled;
        private readonly bool _earlyWarningEnabled;
        private readonly IEventAggregator _eventAggregator;
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly OnEventStatisticMessageCommand _onEventStatisticMessageCommand;
        private readonly OnEventForecastDataMessageCommand _onEventForecastDataMessageCommand;
        private readonly OnEventScheduleMessageCommand _onEventScheduleMessageCommand;
    	private readonly LoadStatisticsAndActualHeadsCommand _loadStatisticsAndActualHeadsCommand;
    	private readonly Queue<MessageForRetryCommand> _messageForRetryQueue = new Queue<MessageForRetryCommand>();

        public IntradayPresenter(IIntradayView view, 
            ISchedulingResultLoader schedulingResultLoader, 
            IMessageBroker messageBroker, 
            IRtaStateHolder rtaStateHolder, 
            IEventAggregator eventAggregator,
            IScheduleDictionarySaver scheduleDictionarySaver,
            IScheduleRepository scheduleRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            OnEventStatisticMessageCommand onEventStatisticMessageCommand, 
            OnEventForecastDataMessageCommand onEventForecastDataMessageCommand, 
            OnEventScheduleMessageCommand onEventScheduleMessageCommand,
			LoadStatisticsAndActualHeadsCommand loadStatisticsAndActualHeadsCommand)
        {
            _eventAggregator = eventAggregator;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scheduleRepository = scheduleRepository;
            _onEventStatisticMessageCommand = onEventStatisticMessageCommand;
            _onEventForecastDataMessageCommand = onEventForecastDataMessageCommand;
            _onEventScheduleMessageCommand = onEventScheduleMessageCommand;
        	_loadStatisticsAndActualHeadsCommand = loadStatisticsAndActualHeadsCommand;
        	_repositoryFactory = repositoryFactory;
            _messageBroker = messageBroker;
            _rtaStateHolder = rtaStateHolder;
            _unitOfWorkFactory = unitOfWorkFactory;
            _view = view;
            _earlyWarningEnabled =
                TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning);
            _realTimeAdherenceEnabled =
                TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence);

            _schedulingResultLoader = schedulingResultLoader;

            _intradayDate = HistoryOnly ? SchedulerStateHolder.RequestedPeriod.DateOnly.StartDate : DateOnly.Today;
        }

        public bool EarlyWarningEnabled
        {
            get { return _earlyWarningEnabled; }
        }

        public bool RealTimeAdherenceEnabled
        {
            get { return _realTimeAdherenceEnabled; }
        }

        /// <summary>
        /// Gets the chart intraday description.
        /// </summary>
        /// <value>The chart intraday description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-21
        /// </remarks>
        public string ChartIntradayDescription
        {
            get { return _chartIntradayDescription; }
        }

        private void listenForMessageBroker()
        {
            if (_messageBroker==null) return;

        	var period = SchedulerStateHolder.RequestedPeriod.Period();
            _messageBroker.RegisterEventSubscription(OnEventStatisticMessageHandler,
                                                    typeof(IStatisticTask));
            _messageBroker.RegisterEventSubscription(OnEventScheduleMessageHandler,
                                                    typeof(IPersistableScheduleData),
                                                    period.StartDateTime,
                                                    period.EndDateTime);
        	_messageBroker.RegisterEventSubscription(OnEventScheduleMessageHandler,
        	                                         typeof (IMeeting));
            _messageBroker.RegisterEventSubscription(OnEventForecastDataMessageHandler,
                                                    typeof(IForecastData),
                                                    period.StartDateTime,
                                                    period.EndDateTime);
            if (!_realTimeAdherenceEnabled || HistoryOnly) return;

            foreach (var person in SchedulerStateHolder.FilteredPersonDictionary.Values)
            {
                _messageBroker.RegisterEventSubscription(OnEventExternalAgentStateMessageHandler,
                                                        person.Id.GetValueOrDefault(),
                                                        typeof(IExternalAgentState),
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddDays(1));
            }

            addSubscriptionForBatchEvents();
        }

    	private void addSubscriptionForBatchEvents()
    	{
    		_messageBroker.RegisterEventSubscription(OnEventExternalAgentStateMessageHandler,
    		                                         Guid.Empty,
    		                                         typeof(IExternalAgentState),
    		                                         DateTime.UtcNow,
    		                                         DateTime.UtcNow.AddDays(1));
    	}

    	public void OnEventExternalAgentStateMessageHandler(object sender, EventMessageArgs e)
        {
            if (e.Message.ModuleId == _moduleId) return;

            ThreadPool.QueueUserWorkItem(handleIncomingExternalEvent, e.Message.DomainObject);
        }

        private void handleIncomingExternalEvent(object state)
        {
            IExternalAgentState externalAgentState =
                    new ExternalAgentStateDecoder().Decode(state as byte[]);

            Logger.DebugFormat("Externalstate received: StateCode {0}, ExternalLogon {1}, Timestamp {2}", externalAgentState.StateCode, externalAgentState.ExternalLogOn, externalAgentState.Timestamp);
            _rtaStateHolder.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            if (ExternalAgentStateReceived != null) ExternalAgentStateReceived.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-20
        /// </remarks>
        public SchedulerStateHolder SchedulerStateHolder
        {
            get { return (SchedulerStateHolder)_schedulingResultLoader.SchedulerState; }
        }

        /// <summary>
        /// Gets the multiplicator definition sets.
        /// </summary>
        /// <value>The multiplicator definition sets.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-02-13
        /// </remarks>
        public IEnumerable<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets
        {
            get { return _schedulingResultLoader.MultiplicatorDefinitionSets; }
        }

        /// <summary>
        /// Gets the rta state holder.
        /// </summary>
        /// <value>The rta state holder.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        public IRtaStateHolder RtaStateHolder
        {
            get { return _rtaStateHolder; }
        }

        /// <summary>
        /// Gets or sets the intraday date.
        /// </summary>
        /// <value>The intraday date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-21
        /// </remarks>
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
                _view.BeginInvoke(new Action<object,EventMessageArgs>(OnEventForecastDataMessageHandler), sender, e);
            }
            else
            {
                if (e.Message.ModuleId == _moduleId) return;

                try
                {
                    _onEventForecastDataMessageCommand.Execute(e.Message);
                }
                catch (DataSourceException)
                {
                    _messageForRetryQueue.Enqueue(new MessageForRetryCommand(_onEventForecastDataMessageCommand,e.Message));
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
                if (e.Message.ModuleId == _moduleId) return;

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
                _view.BeginInvoke(new Action<object,EventMessageArgs>(OnEventStatisticMessageHandler), sender, e );
            }
            else
            {
                if (e.Message.ModuleId == _moduleId) return;

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
            using (PerformanceOutput.ForOperation("Loading the schedules before first use"))
            {
                _rtaStateHolder.InitializeSchedules();
            } 

            if (!_realTimeAdherenceEnabled) return; //If RTA isn't enabled, we don't need the rest of the stuff...

            IStatisticRepository statisticRepository = _repositoryFactory.CreateStatisticRepository();
            using (PerformanceOutput.ForOperation("Read and collect agent states"))
            {
                _rtaStateHolder.CollectAgentStates(
                    statisticRepository.LoadRtaAgentStates(SchedulerStateHolder.RequestedPeriod.Period(),_rtaStateHolder.ExternalLogOnPersons));
            }
            using (PerformanceOutput.ForOperation("Analyzing alarms for initial states"))
            {
                _rtaStateHolder.AnalyzeAlarmSituations(DateTime.UtcNow);
            }
        }

        private void initializeRtaStateHolder()
        {
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingRealTimeAdherenceDataThreeDots);
            
            _rtaStateHolder.Initialize();
            _rtaStateHolder.SetFilteredPersons(SchedulerStateHolder.FilteredPersonDictionary.Values);
            
            if (!_realTimeAdherenceEnabled) return;

            _rtaStateHolder.VerifyDefaultStateGroupExists();
            _rtaStateHolder.RtaStateCreated += _rtaStateHolder_RtaStateCreated;
        }

        private void _rtaStateHolder_RtaStateCreated(object sender, CustomEventArgs<IRtaState> e)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                IRtaStateGroupRepository rtaStateGroupRepository = _repositoryFactory.CreateRtaStateGroupRepository(uow);
                rtaStateGroupRepository.Add(e.Value.StateGroup);

                try
                {
                    uow.PersistAll(this);
                }
                catch (OptimisticLockException)
                {
                    //This is totally ignored as there may be other clients adding the exactly same state any way
                    //Later on we'll have to reload the state groups but as long as someone has saved the new states, this is not an issue.
                }
            }
        }

        /// <summary>
        /// Prepares the chart description.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="descriptions">The descriptions.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-24
        /// </remarks>
        public static string PrepareChartDescription(string format, params string[] descriptions)
        {
            return string.Format(CultureInfo.CurrentCulture, format, descriptions);
        }

        /// <summary>
        /// Prepares the skill intraday collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-22
        /// </remarks>
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

            _loadStatisticsAndActualHeadsCommand.Execute(_intradayDate, skill, skillStaffPeriods);

            // fill in statistic data
            var statistics = new SkillStaffPeriodStatisticsForSkillIntraday(skillStaffPeriods);
            statistics.Analyze();

            return skillStaffPeriods;
        }

        public void UnregisterMessageBrokerEvents()
        {
            if (_messageBroker==null) return;
            _messageBroker.UnregisterEventSubscription(OnEventExternalAgentStateMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventForecastDataMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventScheduleMessageHandler);
            _messageBroker.UnregisterEventSubscription(OnEventStatisticMessageHandler);
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
					var result = _scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository, SchedulerStateHolder.Schedules.DifferenceSinceSnapshot());
                    uow.PersistAll(this);
					if (result != null)
						new ScheduleDictionaryModifiedCallback().Callback(SchedulerStateHolder.Schedules, result.ModifiedEntities, result.AddedEntities, result.DeletedEntities);
				}
            }
            catch (OptimisticLockException optLockEx)
            {
                //rk : temp
                string objText = "[" + optLockEx.EntityName + ":" + optLockEx.RootId + "]";

                _view.ShowInformationMessage(string.Concat(UserTexts.Resources.SomeoneElseHaveChanged + " " + objText + " " +
                         UserTexts.Resources.YourChangesWillBeDiscardedReloading), "  ");

                _view.ToggleSchedulePartModified(false);
                using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    _schedulingResultLoader.ReloadScheduleData(unitOfWork);
                }
                _rtaStateHolder.InitializeSchedules();
                _view.RefreshRealTimeScheduleControls();
                _view.ToggleSchedulePartModified(true);
                _view.DrawSkillGrid();
            }
            _view.UpdateShiftEditor(new List<IScheduleDay>());
            _view.DisableSave();
        }

        public bool CheckIfUserWantsToSaveUnsavedData()
        {
            if (SchedulerStateHolder.Schedules!=null &&
                !SchedulerStateHolder.Schedules.DifferenceSinceSnapshot().IsEmpty())
            {
                DialogResult res = _view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
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

        public Guid ModuleId
        {
            get { return _moduleId; }
        }

        public bool HistoryOnly
        {
            get { return !SchedulerStateHolder.RequestedPeriod.Period().Contains(DateTime.UtcNow); }
        }

        public event EventHandler ExternalAgentStateReceived;

        public void RefreshAgentStates(DateTime timestamp, TimeSpan refreshRate)
        {
            _rtaStateHolder.UpdateCurrentLayers(timestamp, refreshRate);
            _rtaStateHolder.AnalyzeAlarmSituations(timestamp);
        }

        public IDayLayerViewModel CreateDayLayerViewModel()
        {
            var viewModel = new DayLayerViewModel(_rtaStateHolder, _eventAggregator, _unitOfWorkFactory,_repositoryFactory,new DispatcherWrapper());
            viewModel.CreateModels(SchedulerStateHolder.FilteredPersonDictionary.Values,
                                   SchedulerStateHolder.RequestedPeriod);
            return viewModel;
        }

        public IEnumerable<AgentStateViewAdapter> CreateAgentStateViewAdapterCollection()
        {
            foreach (var rtaStateGroup in _rtaStateHolder.RtaStateGroups)
            {
                yield return new AgentStateViewAdapter(_rtaStateHolder, rtaStateGroup);
            }
        }

        public void RetryHandlingMessages()
        {
            while(_messageForRetryQueue.Count>0)
            {
                var handler = _messageForRetryQueue.Peek();
                try
                {
                    handler.Execute();
                }
                catch (DataSourceException dataSourceException)
                {
                    Logger.Error("Something wrong with the data source.",dataSourceException);
                    _view.ShowBackgroundDataSourceError();
                    return;
                }
                catch(Exception exception)
                {
                    Logger.Error("An error occurred!",exception);
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
            UnregisterMessageBrokerEvents();
            _messageForRetryQueue.Clear();
            if (_rtaStateHolder != null)
                _rtaStateHolder.RtaStateCreated -= _rtaStateHolder_RtaStateCreated;
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