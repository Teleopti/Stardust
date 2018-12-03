using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{

    public class AutoPositionedActivityExtender : ActivityExtender, IAutoPositionedActivityExtender
    {
        private byte _numberOfLayers;
        private TimeSpan _startSegment;
        private TimeSpan _autoPositionIntervalSegment;


        public AutoPositionedActivityExtender(IActivity activity,
                                        TimePeriodWithSegment activityLengthWithSegment,
                                        TimeSpan startSegment,
                                        byte numberOfLayers)
            : base(activity, activityLengthWithSegment)
        {
            InParameter.CheckTimeSpanAtLeastOneTick(nameof(startSegment), startSegment);
            _startSegment = startSegment;
            _autoPositionIntervalSegment = startSegment;
            _numberOfLayers = numberOfLayers;
        }

        public AutoPositionedActivityExtender(IActivity activity,
                                              TimePeriodWithSegment activityLengthWithSegment,
                                              TimeSpan startSegment)
                                        : base(activity, activityLengthWithSegment)
        {
            InParameter.CheckTimeSpanAtLeastOneTick(nameof(startSegment), startSegment);
            _startSegment = startSegment;
            _autoPositionIntervalSegment = startSegment;
            _numberOfLayers = 1;
        }

        protected AutoPositionedActivityExtender(){}


        public virtual byte NumberOfLayers
        {
            get
            {
                return _numberOfLayers;
            }
            set
            {
                _numberOfLayers = value;
            }
        }

        public virtual TimeSpan StartSegment
        {
            get
            {
                return _startSegment;
            }
            set
            {
                InParameter.CheckTimeSpanAtLeastOneTick(nameof(value), value);
                _startSegment = value;
            }
        }

        public virtual TimeSpan AutoPositionIntervalSegment
        {
            get
            {
                return _autoPositionIntervalSegment;
            }
            set
            {
                InParameter.CheckTimeSpanAtLeastOneTick(nameof(value), value);
                _startSegment = value;
                _autoPositionIntervalSegment = value;
            }
        }


        public override TimeSpan ExtendMaximum()
        {
            return new TimeSpan(ActivityLengthWithSegment.Period.EndTime.Ticks * _numberOfLayers);
        }


        public override IList<IWorkShift> ReplaceWithNewShifts(IWorkShift shift)
        {
            IList<IWorkShift> retColl = new List<IWorkShift>();
            if(NumberOfLayers==0)
            {
                retColl.Add(shift);
            }
            else
            {
                IVisualLayerCollection shiftProjection = shift.ProjectionService().CreateProjection();
                IList<TimePeriod> inContractPeriods = findInContractPeriods(shiftProjection);

                for (TimeSpan actLength = ActivityLengthWithSegment.Period.StartTime;
                     actLength <= ActivityLengthWithSegment.Period.EndTime;
                     actLength = actLength.Add(ActivityLengthWithSegment.Segment))
                {
                    IDictionary<TimePeriod, int> noOfLayersPutOnPeriod = numberOfLayersInPeriod(actLength, inContractPeriods);
                    if (noOfLayersPutOnPeriod != null)
                        retColl.Add(createNewShift(shift, actLength, noOfLayersPutOnPeriod));
                }   
            }

            return retColl;
        }


        private static TimeSpan calculateOptimalStartTime(TimeSpan activityLength, 
                                                         IDictionary<TimePeriod, int> noOfLayersPutOnPeriod, 
                                                         TimePeriod contractPeriod)
        {
            TimeSpan periodLength = contractPeriod.SpanningTime();
            TimeSpan activityUsedLength = new TimeSpan(activityLength.Ticks * noOfLayersPutOnPeriod[contractPeriod]);
            return new TimeSpan(periodLength.Subtract(activityUsedLength).Ticks /
                                (noOfLayersPutOnPeriod[contractPeriod] + 1));
        }

        private TimeSpan closestSegment(TimeSpan optimalStartTime)
        {
            double segmentTicks = _startSegment.Ticks;
            double noOfSegments = optimalStartTime.Ticks / segmentTicks;

            int noOfSegmentsLow = (int) Math.Floor(noOfSegments);
            int noOfSegmentsHigh = noOfSegmentsLow + 1;
            TimeSpan shiftHigh = new TimeSpan((long)(noOfSegmentsHigh * segmentTicks));
            TimeSpan shiftLow = new TimeSpan((long)(noOfSegmentsLow * segmentTicks));
            long optimalStartTicks = optimalStartTime.Ticks;

            //prio early or late if similar? right now later...
            return shiftHigh.Ticks - optimalStartTicks > optimalStartTicks - shiftLow.Ticks ? shiftLow : shiftHigh;
        }

        private IWorkShift createNewShift(IWorkShift oldShift,
                                         TimeSpan activityLength, 
                                         IDictionary<TimePeriod, int> noOfLayersPutOnPeriod)
        {
            IWorkShift retShift = (IWorkShift) oldShift.Clone();
            foreach (var period in noOfLayersPutOnPeriod)
            {
                TimeSpan contractSegment = calculateOptimalStartTime(activityLength, noOfLayersPutOnPeriod, period.Key);

                TimeSpan startTime = period.Key.StartTime;
                for (int i = 0; i < period.Value; i++)
                {
                    startTime = startTime.Add(contractSegment);
                    TimeSpan correctedStartTime = closestSegment(startTime);
                    retShift.LayerCollection.Add(
                        new WorkShiftActivityLayer(ExtendWithActivity,
                                                   new DateTimePeriod(
                                                       WorkShift.BaseDate.Add(correctedStartTime),
                                                       WorkShift.BaseDate.Add(correctedStartTime.Add(activityLength)))));


                    startTime = startTime.Add(activityLength);
                }
            }
            return retShift;
        }

        private static IList<TimePeriod> findInContractPeriods(IVisualLayerCollection oldShiftAsProjection)
        {
            //put this on IVisualLayerCollection instead?
            IList<TimePeriod> inContractPeriods = new List<TimePeriod>();
            TimeSpan? layerStart = null;
            foreach (IVisualLayer layer in oldShiftAsProjection)
            {
                bool layerInContract = layer.Payload.InContractTime;
                if (layerInContract)
                {
                    if (!layerStart.HasValue)
                        layerStart = layer.Period.StartDateTime.Subtract(WorkShift.BaseDate);
                }
                else
                {
                    if (layerStart.HasValue)
                    {
                        inContractPeriods.Add(new TimePeriod(layerStart.Value, layer.Period.StartDateTime.Subtract(WorkShift.BaseDate)));
                        layerStart = null;
                    }
                }
            }
            if (layerStart != null)
                inContractPeriods.Add(new TimePeriod(layerStart.Value, oldShiftAsProjection.Period().Value.EndDateTime.Subtract(WorkShift.BaseDate)));
            return inContractPeriods;
        }

        private IDictionary<TimePeriod, int> numberOfLayersInPeriod(TimeSpan activityLength, 
                                                                    IEnumerable<TimePeriod> inContractPeriods)
        {
            IDictionary<TimePeriod, int> retDic = new Dictionary<TimePeriod, int>();
            for (int i = 0; i < _numberOfLayers; i++)
            {
                TimePeriod periodMostPlace = new TimePeriod();
                TimeSpan mostTimeLeft = TimeSpan.MinValue;
                foreach (TimePeriod period in inContractPeriods)
                {
                    if (!retDic.ContainsKey(period))
                        retDic[period] = 0;
                    TimeSpan timeLeftOnLayer = calculateOptimalStartTime(activityLength, retDic, period);

                    if (timeLeftOnLayer > mostTimeLeft)
                    {
                        mostTimeLeft = timeLeftOnLayer;
                        periodMostPlace = period;
                    }
                }
                if (mostTimeLeft < activityLength)
                    return null;
                retDic[periodMostPlace]++;
            }
            return retDic;
        }

    }
}