using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Operator enum
    /// </summary>
    public enum OperatorLimiter
    {
        /// <summary>
        /// Less then
        /// </summary>
        LessThen,
        /// <summary>
        /// Greater then
        /// </summary>
        GreaterThen,
        /// <summary>
        /// Equals
        /// </summary>
        Equals,
        /// <summary>
        /// Less then or equals
        /// </summary>
        LessThenEquals,
        /// <summary>
        /// Greater then or equals
        /// </summary>
        GreaterThenEquals
    }

    /// <summary>
    /// Validates that the activity has specified length
    /// </summary>
    public class ActivityTimeLimiter : WorkShiftLimiter
    {
        private IActivity _activity;
        private TimeSpan _timeLimit;   
        private OperatorLimiter _timeLimitOperator;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ActivityTimeLimiter() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="timeLimit"></param>
        /// <param name="timeLimitOperator"></param>
        public ActivityTimeLimiter(IActivity activity, TimeSpan timeLimit, OperatorLimiter timeLimitOperator)
        {
            _activity = activity;
            _timeLimit = timeLimit;
            _timeLimitOperator = timeLimitOperator;
        }

        /// <summary>
        /// Get, set activity time limit
        /// </summary>
        public virtual TimeSpan TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; }
        }

        /// <summary>
        /// Gets or sets the operator for the time limit.
        /// </summary>
        public virtual OperatorLimiter TimeLimitOperator
        {
            get { return _timeLimitOperator; }
            set { _timeLimitOperator = value; }
        }

        /// <summary>
        /// Gets or sets the activity
        /// </summary>
        public virtual IActivity Activity
        {
            get { return _activity; }
            set { _activity = value; }
        }

        /// <summary>
        /// Determines whether [is valid at start] [the specified template].
        /// </summary>
        /// <param name="shift">The template.</param>
        /// <param name="extenders">The extenders.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at start] [the specified template]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders)
        {
            TimeSpan totalExtenderTime = TimeSpan.Zero;
            bool containsActivity = false;

            foreach (IWorkShiftExtender extender in extenders)
            {
                if (extender.ExtendWithActivity.Equals(_activity))
                {
                    containsActivity = true;
                    totalExtenderTime += extender.ExtendMaximum();
                }  
            }

            if (containsActivity)
            {
                if (_timeLimitOperator == OperatorLimiter.GreaterThen)
                {
                    if (totalExtenderTime <= _timeLimit)
                        return false;
                }

                if (_timeLimitOperator == OperatorLimiter.GreaterThenEquals)
                {
                    if (totalExtenderTime < _timeLimit)
                        return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Determines whether [is valid at end] [the specified end projection].
        /// </summary>
        /// <param name="endProjection">The end projection.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at end] [the specified end projection]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValidAtEnd(IVisualLayerCollection endProjection)
        {
            IVisualLayerCollection layers = endProjection.FilterLayers(_activity);
            bool ok = true;

            if(layers.Count() > 0)
            {
                layers = MergeAdjacentLayers(layers);

                foreach (IVisualLayer layer in layers)
                {
                    if (_timeLimitOperator == OperatorLimiter.Equals)
                        ok = (layer.Period.ElapsedTime() == _timeLimit);
                    if (_timeLimitOperator == OperatorLimiter.LessThen)
                        ok = (layer.Period.ElapsedTime() < _timeLimit);
                    if (_timeLimitOperator == OperatorLimiter.GreaterThen)
                        ok =  (layer.Period.ElapsedTime() > _timeLimit);
                    if (_timeLimitOperator == OperatorLimiter.LessThenEquals)
                        ok =  (layer.Period.ElapsedTime() <= _timeLimit);
                    if (_timeLimitOperator == OperatorLimiter.GreaterThenEquals)
                        ok =  (layer.Period.ElapsedTime() >= _timeLimit);

                    if(!ok)
                        return false;     
                }

                return true;
            }

            if (_timeLimitOperator == OperatorLimiter.LessThenEquals || _timeLimitOperator == OperatorLimiter.LessThen)
                return true;
            return false;
        }

        /// <summary>
        /// Merge activityLayers that are adjacent
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        private static IVisualLayerCollection MergeAdjacentLayers(IVisualLayerCollection layers)
        {
            IList<IVisualLayer> retList = new List<IVisualLayer>();
            bool dontCheck = false;

            foreach(IVisualLayer layer in layers)
            {
                if(retList.Count > 0)
                {
                    foreach(IVisualLayer newLayer in retList)
                    {
                        if(newLayer.Period.Intersect(layer.Period))
                            dontCheck = true;
                    }
                }

                if(dontCheck == false)
                    retList.Add(AdjacentLayer(layers,layer));

                dontCheck = false;
            }

            return new VisualLayerCollection(layers.Person, retList, new ProjectionPayloadMerger());
        }

        /// <summary>
        /// Merge
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="layerToCheck"></param>
        /// <returns></returns>
        private static IVisualLayer AdjacentLayer(IEnumerable<IVisualLayer> layers, IVisualLayer layerToCheck)
        {
            IVisualLayer newLayer = layerToCheck;
            IVisualLayerFactory layerFactory = new VisualLayerFactory();
            foreach(IVisualLayer layer in layers)
            {
                if(layer.Period.Adjacent(layerToCheck.Period))
                {
                    DateTimePeriod? union = layer.Period.Union(layerToCheck.Period);
                    if (union.HasValue)
                    {
                        newLayer = layerFactory.CreateResultLayer(layerToCheck.Payload, layerToCheck, union.Value);
                        newLayer = AdjacentLayer(layers, newLayer);
                    }
                }
            }

            return newLayer;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        public override IWorkShiftLimiter NoneEntityClone()
        {
            ActivityTimeLimiter retobj = (ActivityTimeLimiter)MemberwiseClone();
            retobj.SetId(null);
            retobj.SetParent(null);
            return retobj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        public override IWorkShiftLimiter EntityClone()
        {
            return (ActivityTimeLimiter)MemberwiseClone();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return EntityClone();
        }

    }
}
