using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by:SanjayaI
    /// Created date: 5/21/2008
    /// </remarks>
    public class AbsolutePositionView : EntityContainer<ActivityNormalExtender>, ICombinedView
    {
        private TimeSpan _aLSegment;
        private TimeSpan _aLMinTime;
        private TimeSpan _aLMaxTime;
        private TimeSpan _aPSegment;
        private TimeSpan? _aPStartTime;
        private TimeSpan? _aPEndTime;
        private string _currentActivity;
        private IList<IActivity> _activities;

        /// <summary>
        /// Default constructor of the AutoPositionedViewGridData class
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/21/2008
        /// </remarks>
        public AbsolutePositionView()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsolutePositionView"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="activities">The activities.</param>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 5/21/2008
        /// </remarks>
        public AbsolutePositionView(ActivityNormalExtender entity, IList<IActivity> activities)
            : base(entity)
        {
            _activities = activities;

            _aLMinTime = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            _aLMaxTime = ContainedEntity.ActivityLengthWithSegment.Period.EndTime;
            _aLSegment = ContainedEntity.ActivityLengthWithSegment.Segment;

            _aPStartTime = ContainedEntity.ActivityPositionWithSegment.Period.StartTime;
            _aPEndTime = ContainedEntity.ActivityPositionWithSegment.Period.EndTime;
            _aPSegment = ContainedEntity.ActivityPositionWithSegment.Segment;

            _currentActivity = ContainedEntity.ExtendWithActivity.Description.Name;
        }

        /// <summary>
        /// Gets the type of class.
        /// </summary>
        /// <value>The type of class.</value>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 6/15/2008
        /// </remarks>
        public Type TypeOfClass
        {
            get
            {
                Type _type = null;
                if (ContainedEntity.GetType() == typeof(ActivityAbsoluteStartExtender))
                {
                    _type = typeof(ActivityAbsoluteStartExtender);
                }
                else if (ContainedEntity.GetType() == typeof(ActivityRelativeStartExtender))
                {
                    _type = typeof(ActivityRelativeStartExtender);
                }
                else if (ContainedEntity.GetType() == typeof(ActivityRelativeEndExtender))
                {
                    _type = typeof(ActivityRelativeEndExtender);
                }
                return _type;
            }
        }

        /// <summary>
        /// Gets the current extender.
        /// </summary>
        /// <value>The current extender.</value>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 6/15/2008
        /// </remarks>
        public ActivityNormalExtender CurrentExtender
        {
            get { return ContainedEntity; }
        }

        /// <summary>
        /// Description Property
        /// </summary>
        public string Description
        {
            get
            {
                InParameter.NotNull("Description", ContainedEntity.ExtendWithActivity.Description.Name);
                return ContainedEntity.ExtendWithActivity.Description.Name;
            }
            set
            {
                Description _description = new Description(value, string.Empty);
                ContainedEntity.ExtendWithActivity.Description = _description;
            }
        }

        /// <summary>
        /// GroupingActivity Property
        /// </summary>
        public string GroupingActivity
        {
            get
            {
                //InParameter.NotNull("ExtendWithActivity", ContainedEntity.ExtendWithActivity);
                //return ContainedEntity.ExtendWithActivity;
                return _currentActivity;
            }
            set
            {
                //ContainedEntity.ExtendWithActivity = value;
                _currentActivity = value;
            }


        }

        /// <summary>
        /// Gets or sets the length of the activity.
        /// </summary>
        /// <value>The length of the activity.</value>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 6/15/2008
        /// </remarks>
        public TimePeriodWithSegment ActivityLength
        {
            get
            {
                TimePeriodWithSegment _activityLength = ContainedEntity.ActivityLengthWithSegment;
                InParameter.NotNull("ActivityLength", _activityLength);
                return _activityLength;
            }
            set
            {
                ContainedEntity.ActivityLengthWithSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets the activity position.
        /// </summary>
        /// <value>The activity position.</value>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 6/15/2008
        /// </remarks>
        public TimePeriodWithSegment ActivityPosition
        {
            get
            {
                TimePeriodWithSegment _activityPosition = ContainedEntity.ActivityPositionWithSegment;
                InParameter.NotNull("ActivityPosition", _activityPosition);
                return _activityPosition;
            }
            set
            {
                ContainedEntity.ActivityPositionWithSegment = value;
            }
        }

        /// <summary>
        /// StartSegment Property
        /// </summary>
        public TimeSpan StartSegment
        {
            get
            {
                TimeSpan _startSegment = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
                InParameter.NotNull("StartSegment", _startSegment);
                return _startSegment;
            }
            set
            {
                TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(ContainedEntity.ActivityLengthWithSegment.Period,
                    value);
                ContainedEntity.ActivityLengthWithSegment = _timePeriodWithSegment;

            }

        }

        /// <summary>
        /// Gets or sets the start segment for activity position.
        /// </summary>
        /// <value>The start segment for activity position.</value>
        /// <remarks>
        /// Created by: SanjayaI
        /// Created date: 6/15/2008
        /// </remarks>
        public TimeSpan StartSegmentForActivityPosition
        {
            get
            {
                TimeSpan _startSegmentForActivityPosition = ContainedEntity.ActivityPositionWithSegment.Period.StartTime;
                InParameter.NotNull("StartSegmentForActivityPosition", _startSegmentForActivityPosition);
                return _startSegmentForActivityPosition;
            }
            set
            {

                TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(ContainedEntity.ActivityPositionWithSegment.Period,
                  value);
                ContainedEntity.ActivityPositionWithSegment = _timePeriodWithSegment;
            }
        }
        
        /// <summary>
        /// Segment Property
        /// </summary>
        public TimeSpan Segment
        {
            get
            {
                InParameter.NotNull("Segment", ContainedEntity.ActivityLengthWithSegment.Segment);
                return ContainedEntity.ActivityLengthWithSegment.Segment;
            }
            set
            {
                TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(
                                        ContainedEntity.ActivityLengthWithSegment.Period,
                                        value);
                ContainedEntity.ActivityLengthWithSegment = _timePeriodWithSegment;
            }
        }

        /// <summary>
        /// GroupingActivity Property
        /// </summary>
        public TimeSpan StartTime
        {
            get
            {
                InParameter.NotNull("StartTime", ContainedEntity.ActivityLengthWithSegment.Period.StartTime);
                return ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            }
            set
            {
                ContainedEntity.ActivityLengthWithSegment = GetTimePeriodWithSegment(value, SetterMode.StartTime);
            }
        }
        
        /// <summary>
        /// EndTime Property
        /// </summary>
        public TimeSpan EndTime
        {
            get
            {
                InParameter.NotNull("EndTime", ContainedEntity.ActivityLengthWithSegment.Period.EndTime);
                return ContainedEntity.ActivityLengthWithSegment.Period.EndTime;
            }
            set
            {
                ContainedEntity.ActivityLengthWithSegment = GetTimePeriodWithSegment(value, SetterMode.EndTime);
            }
        }

        /// <summary>
        /// Segment Property
        /// </summary>
        public TimeSpan SegmentForActivityPosition
        {
            get
            {
                InParameter.NotNull("SegmentForActivityPosition", ContainedEntity.ActivityPositionWithSegment.Segment);
                return ContainedEntity.ActivityPositionWithSegment.Segment;
            }
            set
            {
                TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(
                                        ContainedEntity.ActivityPositionWithSegment.Period,
                                        value);
                ContainedEntity.ActivityPositionWithSegment = _timePeriodWithSegment;
            }
        }

        /// <summary>
        /// GroupingActivity Property
        /// </summary>
        public TimeSpan StartTimeForActivityPosition
        {
            get
            {
                InParameter.NotNull("StartTime", ContainedEntity.ActivityPositionWithSegment.Period.StartTime);
                return ContainedEntity.ActivityPositionWithSegment.Period.StartTime;
            }
            set
            {
                ContainedEntity.ActivityPositionWithSegment = GetTimePeriodWithSegmentForActivityPosition(value, SetterMode.StartTime);
            }
        }

        /// <summary>
        /// EndTime Property
        /// </summary>
        public TimeSpan EndTimeForActivityPosition
        {
            get
            {
                InParameter.NotNull("EndTime", ContainedEntity.ActivityPositionWithSegment.Period.EndTime);
                return ContainedEntity.ActivityPositionWithSegment.Period.EndTime;
            }
            set
            {
                ContainedEntity.ActivityPositionWithSegment = GetTimePeriodWithSegmentForActivityPosition(value, SetterMode.EndTime);
            }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/27/2008
        /// </remarks>
        public IWorkShiftRuleSet Parent
        {
            get
            {
                return (IWorkShiftRuleSet)ContainedEntity.Parent;
            }
        }

        /// <summary>
        /// Gets the current work shift rule.
        /// </summary>
        /// <value>The current work shift rule.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/27/2008
        /// </remarks>
        public WorkShiftRuleSet CurrentWorkShiftRule
        {
            get
            {
                return ((WorkShiftRuleSet)ContainedEntity.Parent);
            }
        }

        /// <summary>
        /// Gets or sets the work shift rule.
        /// </summary>
        /// <value>The work shift rule.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/27/2008
        /// </remarks>
        public WorkShiftRuleSet WorkShiftRule
        {
            get
            {
                return ((WorkShiftRuleSet)ContainedEntity.Parent);

            }
            set
            {
                ((IWorkShiftExtender)ContainedEntity).SetParent(value);
            }
        }

        #region Methods(2)

        /// <summary>
        /// Returns a shallow copy of the TimePeriodWithSegment
        /// </summary>
        /// <param name="value">a TimeSpan value</param>
        /// <param name="mode">SetterMode value</param>
        internal TimePeriodWithSegment GetTimePeriodWithSegment(TimeSpan value, SetterMode mode)
        {
            TimeSpan startTime = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            TimeSpan endTime = ContainedEntity.ActivityLengthWithSegment.Period.EndTime;

            switch (mode)
            {
                case SetterMode.EndTime:
                    endTime = value;
                    if (startTime > endTime)
                    {
                        endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                    }
                    break;
                case SetterMode.StartTime:
                    startTime = value;
                    if(startTime.Equals(TimeSpan.Zero))
                    {
                        startTime = new TimeSpan(1);
                        endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                    }
                    else if (startTime < endTime)
                    {
                        endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                    }
                    else if (startTime > endTime)
                    {
                        endTime = new TimeSpan(endTime.Hours, endTime.Minutes, endTime.Seconds);
                    }
                    break;
            }

            TimePeriod _timePeriod = new TimePeriod(startTime, endTime);
            TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(_timePeriod, ContainedEntity.ActivityLengthWithSegment.Segment);

            return _timePeriodWithSegment;
        }

        internal TimePeriodWithSegment GetTimePeriodWithSegmentForActivityPosition(TimeSpan value, SetterMode mode)
        {
            TimeSpan _startTime = ContainedEntity.ActivityPositionWithSegment.Period.StartTime;
            TimeSpan _endTime = ContainedEntity.ActivityPositionWithSegment.Period.EndTime;

            switch (mode)
            {
                case SetterMode.EndTime:
                    _endTime = value;
                    if (_endTime.Equals(TimeSpan.Zero))
                        _endTime = new TimeSpan(1);
                    else if (_startTime.Equals(new TimeSpan(1)))
                        _endTime = new TimeSpan(1, _endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    break;
                case SetterMode.StartTime:
                    _startTime = value;
                    if (_startTime.Equals(TimeSpan.Zero))
                    {
                        _startTime = new TimeSpan(1);
                        _endTime = new TimeSpan(1, _endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    }
                    else if (_startTime > _endTime)
                        _endTime = new TimeSpan(1, _endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    else if (_startTime < _endTime)
                        _endTime = new TimeSpan(0, _endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    
                    break;
            }

            TimePeriod _timePeriod = new TimePeriod(_startTime, _endTime);
            TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(_timePeriod, ContainedEntity.ActivityPositionWithSegment.Segment);

            return _timePeriodWithSegment;
        }

        #endregion

        #region ICombinedView Members

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public WorkShiftRuleSet RuleSet
        {
            get
            {
                return WorkShiftRule;
            }
            set
            {
                this.WorkShiftRule = value;
            }
        }

        private bool _isAutoPosition;
        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public bool IsAutoPosition
        {
            get
            {
                return _isAutoPosition;
            }
            set
            {
                if(this.Validate())
                {
                    _isAutoPosition = value;
                    if (_isAutoPosition)
                    {
                        CombinedViewType viewType = CombinedViewType.AutoPosition;
                        OnTypeChanged(new CombinedViewTypeChangeEventArgs(viewType, this));
                    }    
                }
                
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public object CVCount
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public Type TypeClass
        {
            get
            {
                return TypeOfClass;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public string CurrentActivity
        {
            get
            {
                return GroupingActivity;
            }
            set
            {
                GroupingActivity = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan ALSegment
        {
            get
            {
                return _aLSegment;
            }
            set
            {
                _aLSegment = value;
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan ALMinTime
        {
            get
            {
                return _aLMinTime;
            }
            set
            {
                _aLMinTime = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan ALMaxTime
        {
            get
            {
                return _aLMaxTime;
            }
            set
            {
                _aLMaxTime = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan APSegment
        {
            get
            {
                return _aPSegment;
            }
            set
            {
                _aPSegment = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan? APStartTime
        {
            get
            {
                return _aPStartTime;
            }
            set
            {
                _aPStartTime = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public TimeSpan? APEndTime
        {
            get
            {
                return _aPEndTime;
            }
            set
            {
                _aPEndTime = value;
            }
        }

        /// <summary>
        /// Gets the work shift extender.
        /// </summary>
        /// <value>The work shift extender.</value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public IWorkShiftExtender WorkShiftExtender
        {
            get
            {
                return this.ContainedEntity;
            }
        }

        /// <summary>
        /// Gets or sets the work shift rule set.
        /// </summary>
        /// <value>The work shift rule set.</value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public WorkShiftRuleSet WorkShiftRuleSet
        {
            get
            {
                return WorkShiftRule;
            }
            set
            {
                WorkShiftRule = value;
            }
        }

        /// <summary>
        /// Occurs when [_type changed].
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private event EventHandler<CombinedViewTypeChangeEventArgs> _typeChanged;

        /// <summary>
        /// Occurs when [type changed].
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public event EventHandler<CombinedViewTypeChangeEventArgs> TypeChanged
        {
            add
            {
                _typeChanged += value;
            }
            remove
            {
                _typeChanged -= value;
            }
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="e">The <see cref="Teleopti.Ccc.WinCode.ShiftCreator.CombinedViewTypeChangeEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        protected virtual void OnTypeChanged(CombinedViewTypeChangeEventArgs e)
        {
            if (_typeChanged != null)
                _typeChanged(this, e);
        }
        #endregion

        #region IGridDataValidator Members

        /// <summary>
        /// Validates the time.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private bool ValidateTime()
        {
            bool status = true;

            if(_aLMinTime.Equals(TimeSpan.Zero))
                status &= false;
            if (status && _aLMaxTime.Equals(TimeSpan.Zero))
                status &= false;
            if (status && (_aLMinTime > _aLMaxTime))
                status &= false;
            if (_aLSegment.Equals(TimeSpan.Zero))
                status &= false;

            if (status && _aPStartTime > _aPEndTime)
                status &= false;
            if (status && _aPSegment.Equals(TimeSpan.Zero))
                status &= false;

            if(status)
            {
                TimePeriodWithSegment alTimePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod(_aLMinTime, 
                                                                                                         _aLMaxTime), 
                                                                                                        _aLSegment);
                TimePeriodWithSegment apTimePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod((TimeSpan)_aPStartTime, 
                                                                                                         (TimeSpan)_aPEndTime), 
                                                                                                         (TimeSpan)_aPSegment);

                ContainedEntity.ActivityLengthWithSegment = alTimePeriodWithSegment;
                ContainedEntity.ActivityPositionWithSegment = apTimePeriodWithSegment;
                ContainedEntity.ExtendWithActivity = (from p in _activities where p.Name.Equals(_currentActivity) select p).First();
            }

            return status;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public bool Validate()
        {
            return ValidateTime();
        }

        #endregion
    }
}
