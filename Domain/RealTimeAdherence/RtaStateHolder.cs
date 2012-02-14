using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaStateHolder : IRtaStateHolder
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IRtaStateGroupProvider _rtaStateGroupProvider;
        private readonly IStateGroupActivityAlarmProvider _stateGroupActivityAlarmProvider;
        private IEnumerable<IRtaStateGroup> _rtaStateGroups;
        private IEnumerable<IStateGroupActivityAlarm> _stateGroupActivityAlarms;
        private readonly IRangeProjectionService _rangeProjectionService;
        private readonly IDictionary<IPerson,IAgentState> _agentStates = new Dictionary<IPerson,IAgentState>();
        private readonly IDictionary<BatchIdentifier,IAgentStateBatch> _agentStateBatchDictionary = new Dictionary<BatchIdentifier, IAgentStateBatch>();
        private IList<IActivity> _rtaStateAsActivities = new List<IActivity>();
        private readonly DateOnlyPeriod _dateOnlyPeriodToday = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
        private readonly IList<ExternalLogOnPerson> _externalLogOnPersons = new List<ExternalLogOnPerson>();
        private static readonly object LockObject = new object();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RtaStateHolder));
        
        public event EventHandler<CustomEventArgs<IRtaState>> RtaStateCreated;

        public RtaStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, IRtaStateGroupProvider rtaStateGroupProvider, IStateGroupActivityAlarmProvider stateGroupActivityAlarmProvider, IRangeProjectionService rangeProjectionService)
        {
            InParameter.NotNull("schedulingResultStateHolder", schedulingResultStateHolder);
            InParameter.NotNull("rtaStateGroupProvider", rtaStateGroupProvider);
            InParameter.NotNull("stateGroupActivityAlarmProvider", stateGroupActivityAlarmProvider);

            _schedulingResultStateHolder = schedulingResultStateHolder;
            _rtaStateGroupProvider = rtaStateGroupProvider;
            _stateGroupActivityAlarmProvider = stateGroupActivityAlarmProvider;
            _rangeProjectionService = rangeProjectionService;
        }

        public void Initialize()
        {
            _rtaStateGroups = _rtaStateGroupProvider.StateGroups();
            _rtaStateAsActivities = (from g in _rtaStateGroups
                                     from s in g.StateCollection
                                     select (IActivity)new Activity("dummy") { InContractTime = false, Description = new Description(s.Name, s.StateCode) }).ToList();
            _stateGroupActivityAlarms = _stateGroupActivityAlarmProvider.StateGroupActivityAlarms();
        }

        public IEnumerable<IRtaStateGroup> RtaStateGroups
        {
            get { return _rtaStateGroups; }
        }

        public IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms
        {
            get { return _stateGroupActivityAlarms; }
        }

        public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _schedulingResultStateHolder; }
        }

        public IDictionary<IPerson, IAgentState> AgentStates
        {
            get { return new ReadOnlyDictionary<IPerson,IAgentState>(_agentStates); }
        }

        public IList<ExternalLogOnPerson> ExternalLogOnPersons
        {
            get { return _externalLogOnPersons; }
        }

        public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
        {
            lock (LockObject)
            {
                _externalLogOnPersons.Clear();
                foreach (IPerson person in filteredPersons)
                {
                    foreach (IPersonPeriod personPeriod in person.PersonPeriods(_dateOnlyPeriodToday))
                    {
                        foreach (IExternalLogOn externalLogOn in personPeriod.ExternalLogOnCollection)
                        {
                            _externalLogOnPersons.Add(new ExternalLogOnPerson { ExternalLogOn = externalLogOn.AcdLogOnOriginalId.Trim(), DataSourceId = externalLogOn.DataSourceId, Person = person });
                        }
                    }
                }                
            }
        }

        public void InitializeSchedules()
        {
            lock (LockObject)
            {
                foreach (var agentState in _agentStates.Values)
                {
                    agentState.SetSchedule(_schedulingResultStateHolder.Schedules);
                }                
            }
        }

        public void CollectAgentStates(IEnumerable<IExternalAgentState> externalAgentStates)
        {
            foreach (var externalAgentState in externalAgentStates)
            {
                lock (LockObject)
                {
                    if (string.IsNullOrEmpty(externalAgentState.ExternalLogOn) &&
                        string.IsNullOrEmpty(externalAgentState.StateCode) &&
                        externalAgentState.IsSnapshot)
                    {
                        HandleEndOfBatch(externalAgentState);
                        continue;
                    }

                    IPerson person = FindPersonByExternalLogOn(externalAgentState);
                    if (person == null) continue;

                    IRtaState state = FindRtaState(externalAgentState);
                    if (state == null) continue;

                    IActivity dummyActivity = FindDummyActivityForState(state);

                    if (externalAgentState.IsSnapshot)
                        InitializeAgentStateBatch(
                            new BatchIdentifier
                                {
                                    BatchTimestamp = externalAgentState.BatchId,
                                    DataSourceId = externalAgentState.DataSourceId
                                }, person);

                    if (_agentStateBatchDictionary.Count > 0 && !externalAgentState.IsSnapshot)
                        AddPersonToLastBatch(person);

                    IAgentState agentState = FindAgentState(person);
                    if (state.StateGroup.IsLogOutState)
                    {
                        agentState.LogOff(externalAgentState.Timestamp.Add(externalAgentState.TimeInState.Negate()));
                    }
                    else
                    {
                        agentState.LengthenOrCreateLayer(externalAgentState, state, dummyActivity);
                    }
                }
            }
        }

        private void AddPersonToLastBatch(IPerson person)
        {
            IAgentStateBatch lastBatch =
                    _agentStateBatchDictionary.LastOrDefault().Value;
            lastBatch.AddPerson(person);
        }

        private IActivity FindDummyActivityForState(IRtaState state)
        {
            IActivity activity = _rtaStateAsActivities.FirstOrDefault(a => a.Description.ShortName == state.StateCode);
            if (activity != null) return activity;
            activity = new Activity("dummy") {InContractTime = false,Description = new Description(state.Name, state.StateCode)};
            _rtaStateAsActivities.Add(activity);
            return activity;
        }

        private IAgentState FindAgentState(IPerson person)
        {
            IAgentState agentState;
            if (_agentStates.TryGetValue(person, out agentState))
                return agentState;

            agentState = new AgentState(person, _rangeProjectionService);
            agentState.SetSchedule(_schedulingResultStateHolder.Schedules);
            _agentStates.Add(person, agentState);
            return agentState;
        }

        private IPerson FindPersonByExternalLogOn(IExternalAgentState agentState)
        {
            ExternalLogOnPerson externalLogOnPerson =
                _externalLogOnPersons.FirstOrDefault(
                    e => e.ExternalLogOn == agentState.ExternalLogOn && e.DataSourceId == agentState.DataSourceId);
            return externalLogOnPerson==null ? null : externalLogOnPerson.Person;
        }

        private void InitializeAgentStateBatch(BatchIdentifier batchIdentifier, IPerson person)
        {
            IAgentStateBatch agentStateBatch;
            if(_agentStateBatchDictionary.TryGetValue(batchIdentifier,out agentStateBatch))
            {
                agentStateBatch.AddPerson(person);
            }
            else
            {
                agentStateBatch = new AgentStateBatch(batchIdentifier);
                agentStateBatch.AddPerson(person);
                _agentStateBatchDictionary.Add(batchIdentifier,agentStateBatch);
            }
        }

        private void HandleEndOfBatch(IExternalAgentState stateEndOfBatch)
        {
            var batchIdentifier = new BatchIdentifier
                                      {
                                          BatchTimestamp = stateEndOfBatch.BatchId,
                                          DataSourceId = stateEndOfBatch.DataSourceId
                                      };
            IAgentStateBatch lastBatch;
            if (!_agentStateBatchDictionary.TryGetValue(batchIdentifier,out lastBatch))
            {
                lastBatch = new AgentStateBatch(batchIdentifier);
                _agentStateBatchDictionary.Add( batchIdentifier,lastBatch);
            }
            IAgentStateBatch previousBatch =
                    _agentStateBatchDictionary.LastOrDefault(b => b.Key.BatchTimestamp < batchIdentifier.BatchTimestamp && 
                                                                  b.Key.DataSourceId == batchIdentifier.DataSourceId).Value;
            if (previousBatch != null)
            {
                LogOutPersons(lastBatch.CompareWithPreviousBatch(previousBatch), batchIdentifier.BatchTimestamp);
            }
        }

        private void LogOutPersons(IEnumerable<IPerson> persons, DateTime identifier)
        {
            foreach (var person in persons)
            {
                IAgentState agentState;
                if (_agentStates.TryGetValue(person, out agentState))
                {
                    agentState.LogOff(identifier);
                }
            }
        }

        private IRtaState FindRtaState(IExternalAgentState agentState)
        {
            var foundState = (from stateGroup in _rtaStateGroups
                              from state in stateGroup.StateCollection
                              where state.PlatformTypeId == agentState.PlatformTypeId &&
                                    state.StateCode == agentState.StateCode
                              select state).FirstOrDefault();
            if (foundState != null) return foundState;

            var defaultStateGroup = _rtaStateGroups.FirstOrDefault(s => s.DefaultStateGroup);
            if (defaultStateGroup!=null)
            {
                foundState = defaultStateGroup.AddState(agentState.StateCode,agentState.StateCode,agentState.PlatformTypeId);
                OnRtaStateCreated(new CustomEventArgs<IRtaState>(foundState));
                return foundState;
            }

            Logger.WarnFormat("Could not find a default state group. State not added. State code: {0}, Platform id: {1}",agentState.StateCode,agentState.PlatformTypeId);
            return null;
        }

        private void OnRtaStateCreated(CustomEventArgs<IRtaState> eventArgs)
        {
            if (RtaStateCreated!=null)
            {
                RtaStateCreated.Invoke(this,eventArgs);
            }
        }

        public void UpdateCurrentLayers(DateTime timestamp, TimeSpan refreshRate)
        {
            foreach (ExternalLogOnPerson externalLogOnPerson in _externalLogOnPersons)
            {
                lock (LockObject)
                {
                    IAgentState agentState = FindAgentState(externalLogOnPerson.Person);
                    agentState.UpdateCurrentLayer(timestamp, refreshRate);
                }
            }
        }

        public void AnalyzeAlarmSituations(DateTime timestamp)
        {
            foreach (ExternalLogOnPerson externalLogOnPerson in _externalLogOnPersons)
            {
                lock (LockObject)
                {
                    IAgentState agentState = FindAgentState(externalLogOnPerson.Person);
                    agentState.AnalyzeAlarmSituations(_stateGroupActivityAlarms, timestamp);
                }
            }
        }

        public void VerifyDefaultStateGroupExists()
        {
            if (!_rtaStateGroups.Any())
                throw new DefaultStateGroupException("The RTA state groups must be configured first.");
        }
    }

    public interface IStateGroupActivityAlarmProvider
    {
        IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms();
    }

    public class StateGroupActivityAlarmProvider : IStateGroupActivityAlarmProvider
    {
        private readonly IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;

        public StateGroupActivityAlarmProvider(IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository)
        {
            _stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
        }

        public IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms()
        {
            return _stateGroupActivityAlarmRepository.LoadAllCompleteGraph();
        }
    }

    public interface IRtaStateGroupProvider
    {
        IEnumerable<IRtaStateGroup> StateGroups();
    }

    public class RtaStateGroupProvider : IRtaStateGroupProvider
    {
        private readonly IRtaStateGroupRepository _rtaStateGroupRepository;

        public RtaStateGroupProvider(IRtaStateGroupRepository rtaStateGroupRepository)
        {
            _rtaStateGroupRepository = rtaStateGroupRepository;
        }

        public IEnumerable<IRtaStateGroup> StateGroups()
        {
            return _rtaStateGroupRepository.LoadAllCompleteGraph();
        }
    }
}