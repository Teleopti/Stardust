using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class AgentState : IAgentState
    {
        private readonly IPerson _person;
        private readonly IRangeProjectionService _rangeProjectionService;
        private readonly IList<IRtaVisualLayer> _rtaStateLayerCollection = new List<IRtaVisualLayer>();
        private readonly IList<IVisualLayer> _alarmSituationCollection = new List<IVisualLayer>();
        private IEnumerable<IVisualLayer> _visualLayerCollection;
        private static readonly object LockObject = new object();

        public AgentState(IPerson person, IRangeProjectionService rangeProjectionService)
        {
            _person = person;
            _rangeProjectionService = rangeProjectionService;
        }

        public IPerson Person { get { return _person; } }

        public void AddRtaVisualLayer(IRtaVisualLayer rtaVisualLayer)
        {
            lock (LockObject)
            {
                _rtaStateLayerCollection.Add(rtaVisualLayer);
            }
        }

        public void LengthenOrCreateLayer(IExternalAgentState externalAgentState, IRtaState rtaState, IActivity dummyActivityForState)
        {
            DateTime timestamp = externalAgentState.Timestamp.Add(externalAgentState.TimeInState.Negate());
            if (timestamp > externalAgentState.Timestamp)
                timestamp = externalAgentState.Timestamp;

            IRtaVisualLayer rtaVisualLayer = GetPreviousStateLayer(timestamp);
            if (rtaVisualLayer == null ||
                (!rtaVisualLayer.Payload.Equals(rtaState.StateGroup) || rtaVisualLayer.IsLoggedOut))
            {
                DateTime endTimeForLayer = externalAgentState.Timestamp;

                //New layer
                if (rtaVisualLayer != null &&
                    !rtaVisualLayer.IsLoggedOut &&
                    rtaVisualLayer.Period.EndDateTime > timestamp.AddHours(-6))
                {
                    if (rtaVisualLayer.Period.EndDateTime > endTimeForLayer)
                        endTimeForLayer = rtaVisualLayer.Period.EndDateTime;

                    if (rtaVisualLayer.Period.StartDateTime <= timestamp)
                    {
                        rtaVisualLayer.Period = new DateTimePeriod(rtaVisualLayer.Period.StartDateTime,
                                                                   timestamp);
                    }
                }
                rtaVisualLayer = new RtaVisualLayer(rtaState,
                                                    new DateTimePeriod(timestamp, endTimeForLayer),
                                                    dummyActivityForState, Person);
                AddRtaVisualLayer(rtaVisualLayer);
            }
            else
            {
                //Lengthen layer
                if (rtaVisualLayer.Period.EndDateTime < timestamp)
                    rtaVisualLayer.Period = new DateTimePeriod(rtaVisualLayer.Period.StartDateTime, timestamp);
            }
        }

        private IRtaVisualLayer GetPreviousStateLayer(DateTime timestamp)
        {
            DateTime start = timestamp.AddHours(-6);

            IRtaVisualLayer layerToReturn = null;
            lock (LockObject)
            {
                for (int i = _rtaStateLayerCollection.Count - 1; i >= 0; i--)
                {
                    DateTimePeriod currentPeriod = _rtaStateLayerCollection[i].Period;
                    if (currentPeriod.EndDateTime < start) break;
                    if (currentPeriod.EndDateTime >= start)
                    {
                        layerToReturn = _rtaStateLayerCollection[i];
                        break;
                    }
                }
            }
            return layerToReturn;
        }

        private DateTime RemoveCurrentAlarmLayersAndReturnStartingPoint(DateTime timestamp)
        {
            DateTime startingPoint = timestamp;
            if (_alarmSituationCollection.Count==0 &&
                _rtaStateLayerCollection.Count>0)
            {
                if (startingPoint > _rtaStateLayerCollection[0].Period.StartDateTime)
                startingPoint = _rtaStateLayerCollection[0].Period.StartDateTime;
            }
            if(_alarmSituationCollection.Count==0 &&
                !_visualLayerCollection.IsEmpty())
            {
                if (startingPoint > _visualLayerCollection.ElementAt(0).Period.StartDateTime)
                    startingPoint = _visualLayerCollection.ElementAt(0).Period.StartDateTime;
            }
            int lastTwo = 0;
            for (int i = _alarmSituationCollection.Count-1; i >= 0; i--)
            {
                DateTimePeriod currentPeriod = _alarmSituationCollection[i].Period;
                startingPoint = currentPeriod.StartDateTime;
                _alarmSituationCollection.RemoveAt(i);
                if (lastTwo>2)
                {
                    break;
                }
                if (startingPoint<timestamp)
                {
                    lastTwo++;
                }
            }
            return startingPoint;
        }

        public void SetSchedule(IScheduleDictionary scheduleDictionary)
        {
            IScheduleRange scheduleRange = scheduleDictionary[_person];
            _visualLayerCollection = _rangeProjectionService.CreateProjection(scheduleRange,
                                                                              new DateTimePeriod(
                                                                                  DateTime.UtcNow.AddDays(-1),
                                                                                  DateTime.UtcNow.AddDays(1)));
        }

        public void ClearAlarmSituations()
        {
            _alarmSituationCollection.Clear();
        }

        public void AnalyzeAlarmSituations(IEnumerable<IStateGroupActivityAlarm> stateGroupActivityAlarms, DateTime timestamp)
        {
            if (_visualLayerCollection == null)
                throw new InvalidOperationException("The schedule must be set prior to using this method.");

            DateTime startingPoint;
            IList<DateTimePeriod> combinedDateTimePeriods;
            lock (LockObject)
            {
                startingPoint = RemoveCurrentAlarmLayersAndReturnStartingPoint(timestamp);

                var allPeriods = _visualLayerCollection.OfType<IPeriodized>().Concat(_rtaStateLayerCollection.OfType<IPeriodized>());
                combinedDateTimePeriods = SkillDayStaffHelper.CreateUniqueDateTimePeriodList(allPeriods);
            }

            AlarmSituation currentLayer = null;
            foreach (DateTimePeriod period in combinedDateTimePeriods.Where(p => p.EndDateTime > startingPoint && p.StartDateTime < timestamp))
            {
                DateTime periodStartDateTime = period.StartDateTime;
                if (startingPoint > periodStartDateTime) periodStartDateTime = startingPoint;
                IVisualLayer scheduleLayer = _visualLayerCollection.FirstOrDefault(l => l.Period.Contains(periodStartDateTime));
                IVisualLayer stateLayer = FindCurrentState(periodStartDateTime);

                IPayload activity = null;
                IPayload stateGroup = null;
                if (scheduleLayer!=null) activity = scheduleLayer.Payload.UnderlyingPayload;
                if (stateLayer != null) stateGroup = stateLayer.Payload;

                IAlarmType alarmType = null;
                foreach (IStateGroupActivityAlarm stateGroupActivityAlarm in stateGroupActivityAlarms)
                {
                    if ((stateGroupActivityAlarm.Activity == activity || (activity!=null && activity.Equals(stateGroupActivityAlarm.Activity))) &&
                        (stateGroupActivityAlarm.StateGroup == stateGroup || (stateGroup != null && stateGroup.Equals(stateGroupActivityAlarm.StateGroup))))
                    {
                        alarmType = stateGroupActivityAlarm.AlarmType;
                        break;
                    }
                }

                if (alarmType==null) continue;
                if (alarmType.ThresholdTime > period.ElapsedTime()) continue;
                DateTime alarmTimeEnd = period.EndDateTime;
                if (alarmTimeEnd>timestamp) alarmTimeEnd = timestamp.AddTicks(1);

                if (currentLayer!=null && currentLayer.Payload.Equals(alarmType) &&
                    currentLayer.Period.EndDateTime == periodStartDateTime &&
                    currentLayer.Period.EndDateTime < alarmTimeEnd)
                {
                    currentLayer.Period = new DateTimePeriod(currentLayer.Period.StartDateTime, alarmTimeEnd);
                }
                else
                {
                    currentLayer = new AlarmSituation(alarmType, new DateTimePeriod(periodStartDateTime, alarmTimeEnd), Person);
                    _alarmSituationCollection.Add(currentLayer);
                }
            }
        }

        public void UpdateCurrentLayer(DateTime timestamp, TimeSpan refreshRate)
        {
            IRtaVisualLayer previousLayer = GetPreviousStateLayer(timestamp);
            if (previousLayer == null || previousLayer.IsLoggedOut) return;

            DateTime endTimeForLayer = timestamp.Add(refreshRate).AddTicks(1);
            if (previousLayer.Period.EndDateTime < endTimeForLayer)
            {
                previousLayer.Period = new DateTimePeriod(previousLayer.Period.StartDateTime,
                                                          endTimeForLayer);
            }
        }

        public IVisualLayer FindCurrentAlarm(DateTime timestamp)
        {
            return _alarmSituationCollection.FirstOrDefault(l => l.Period.Contains(timestamp));
        }

        public IRtaVisualLayer FindCurrentState(DateTime timestamp)
        {
            lock (LockObject)
            {
                foreach (IRtaVisualLayer d in _rtaStateLayerCollection.Reverse())
                {
                    if (d.Period.Contains(timestamp))
                        return d;
                }                
            }
            return null;
        }

        public IVisualLayer FindCurrentSchedule(DateTime timestamp)
        {
            foreach (IVisualLayer d in _visualLayerCollection.Reverse())
            {
                if (d.Period.Contains(timestamp))
                    return d;
            }
            return null;
        }

        public IVisualLayer FindNextSchedule(DateTime timestamp)
        {
            IVisualLayer nextLayer = null;
            foreach (IVisualLayer d in _visualLayerCollection.Reverse())
            {
                if (d.Period.StartDateTime<=timestamp) break;
                nextLayer = d;
            }
            return nextLayer;
        }

        public void LogOff(DateTime timestamp)
        {
            IRtaVisualLayer currentLayer = GetPreviousStateLayer(timestamp);
            if (currentLayer == null || currentLayer.IsLoggedOut) return;

            if (timestamp < currentLayer.Period.EndDateTime)
            {
                timestamp = currentLayer.Period.EndDateTime;
            }
            currentLayer.Period = new DateTimePeriod(currentLayer.Period.StartDateTime, timestamp);
            currentLayer.IsLoggedOut = true;
        }

        public ReadOnlyCollection<IRtaVisualLayer> RtaVisualLayerCollection
        {
            get { return new ReadOnlyCollection<IRtaVisualLayer>(_rtaStateLayerCollection); }
        }

        public ReadOnlyCollection<IVisualLayer> AlarmSituationCollection
        {
            get { return new ReadOnlyCollection<IVisualLayer>(_alarmSituationCollection); }
        }
    }
}