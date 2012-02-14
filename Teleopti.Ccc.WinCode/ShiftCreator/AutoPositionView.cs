using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{
    /// <summary>
    /// SetterMode
    /// </summary>
    public enum SetterMode
    {
        /// <summary>
        /// Start Time
        /// </summary>
        StartTime,
        /// <summary>
        /// End Time
        /// </summary>
        EndTime,
    }

    /// <summary>
    /// Data source for AutoPositioned Grid in Shift Creator
    /// </summary>
    /// <remarks>
    /// Created by: Viraj Damith Siriwardana
    /// Created date: 2008-05-15
    /// </remarks>
    public class AutoPositionView : EntityContainer<IWorkShiftExtender>, ICombinedView, IGridDataValidator
    {
        private string _currentActivity;
        private IList<IActivity> _activities;
        private TimeSpan _aPSegment;
        
        /// <summary>
        /// Default constructor of the AutoPositionGridViewAdapter class
        /// </summary>
        public AutoPositionView()
        {
            
        }

        /// <summary>
        /// Constructor of the AutoPositionGridViewAdapter class
        /// </summary>
        /// <param name="entity">Accepts an instance of type IWorkShiftExtender</param>
        /// <param name="activities">The activities.</param>
        public AutoPositionView(IWorkShiftExtender entity, IList<IActivity> activities)
            : base(entity)
        {
            _activities = activities;

            _alMinTime = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            _alMaxTime = ContainedEntity.ActivityLengthWithSegment.Period.EndTime;
            _alSegment = ContainedEntity.ActivityLengthWithSegment.Segment; 
            
            _aPSegment = ContainedEntity.ActivityLengthWithSegment.Period.StartTime; 

            _currentActivity = ContainedEntity.ExtendWithActivity.Description.Name;
        }

        /// <summary>
        /// GroupingActivity Property
        /// </summary>
        /// <value>The grouping activity.</value>
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
        /// Parent Property
        /// </summary>
        /// <value>The parent.</value>
        public IWorkShiftRuleSet Parent
        {
            get
            {
                return (IWorkShiftRuleSet)ContainedEntity.Parent;
            }
            set
            {
                InParameter.NotNull("Parent", value);
                ContainedEntity.SetParent(value);
            }
        }

        /// <summary>
        /// Count Property
        /// </summary>
        /// <value>The count.</value>
        public byte Count
        {
            get
            {
                return ((AutoPositionedActivityExtender) this.ContainedEntity).NumberOfLayers;
            }
            set
            {
                ((AutoPositionedActivityExtender) this.ContainedEntity).NumberOfLayers = value;
            }
        }

        /// <summary>
        /// Segment Property
        /// </summary>
        /// <value>The segment.</value>
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
                                        value
                                        );
                ContainedEntity.ActivityLengthWithSegment = _timePeriodWithSegment;
            }
        }

        /// <summary>
        /// StartTime Property
        /// </summary>
        /// <value>The start time.</value>
        public TimeSpan StartTime
        {
            get
            {
                InParameter.NotNull("StartTime", ContainedEntity.ActivityLengthWithSegment.Period.StartTime);
                return ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            }
            set
            {
                ContainedEntity.ActivityLengthWithSegment = this.GetTimePeriodWithSegment(value, SetterMode.StartTime);
            }
        }

        /// <summary>
        /// EndTime Property
        /// </summary>
        /// <value>The end time.</value>
        public TimeSpan EndTime
        {
            get
            {
                InParameter.NotNull("EndTime", ContainedEntity.ActivityLengthWithSegment.Period.EndTime);
                return ContainedEntity.ActivityLengthWithSegment.Period.EndTime;
            }
            set
            {
                ContainedEntity.ActivityLengthWithSegment = this.GetTimePeriodWithSegment(value, SetterMode.EndTime);
            }
        }

        /// <summary>
        /// WorkShiftRuleSet Property
        /// </summary>
        /// <value>The work shift rule.</value>
        public WorkShiftRuleSet WorkShiftRule
        {
            get
            {
                return ((WorkShiftRuleSet) ContainedEntity.Parent);
            }
            set
            {
                ContainedEntity.SetParent(value);
            }
        }

        /// <summary>
        /// CurrentWorkShiftRule Property
        /// </summary>
        /// <value>The current work shift rule.</value>
        public WorkShiftRuleSet CurrentWorkShiftRule
        {
            get
            {
                return ((WorkShiftRuleSet)ContainedEntity.Parent);
            }
            set
            {
                ContainedEntity.SetParent(value);
            }
        }


        #region Methods(1)

        /// <summary>
        /// Returns a shallow copy of the TimePeriodWithSegment
        /// </summary>
        /// <param name="value">a TimeSpan value</param>
        /// <param name="mode">SetterMode value</param>
        internal TimePeriodWithSegment GetTimePeriodWithSegment(TimeSpan value, SetterMode mode)
        {
            TimePeriod _timePeriod;
            TimeSpan _startTime;
            TimeSpan _endTime;
            TimePeriodWithSegment _timePeriodWithSegment;

            _startTime = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            _endTime = ContainedEntity.ActivityLengthWithSegment.Period.EndTime;

            switch(mode)
            {
                case SetterMode.EndTime:
                    _endTime = value;
                    if (_startTime > _endTime)
                    {
                        _endTime = new TimeSpan(_endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    }
                    break;
                case SetterMode.StartTime:
                    _startTime = value;
                    if (_startTime.Equals(TimeSpan.Zero))
                    {
                        _startTime = new TimeSpan(1);
                        _endTime = new TimeSpan(_endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    }
                    else if (_startTime < _endTime)
                    {
                        _endTime = new TimeSpan(_endTime.Hours, _endTime.Minutes, _endTime.Seconds);
                    }
                    break;
            }

            _timePeriod = new TimePeriod(_startTime, _endTime);
            _timePeriodWithSegment = new TimePeriodWithSegment(_timePeriod, ContainedEntity.ActivityLengthWithSegment.Segment);

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

        private bool _isAutoPosition = true;
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
                if (this.Validate())
                {
                    _isAutoPosition = value;
                    CombinedViewType viewType = CombinedViewType.AbsolutePosition;
                    if (_isAutoPosition)
                        viewType = CombinedViewType.AutoPosition;
                    OnTypeChanged(new CombinedViewTypeChangeEventArgs(viewType, this));
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
                return Count;
            }
            set
            {
                byte nValue;
                if (Byte.TryParse(value.ToString(), out nValue))
                    Count = nValue;
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
                return null;
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
        private TimeSpan _alSegment;
        public TimeSpan ALSegment
        {
            get
            {
                return _alSegment;
            }
            set
            {
                _alSegment = value;
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private TimeSpan _alMinTime;
        public TimeSpan ALMinTime
        {
            get
            {
                return _alMinTime;
            }
            set
            {
                _alMinTime = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private TimeSpan _alMaxTime;
        public TimeSpan ALMaxTime
        {
            get
            {
                return _alMaxTime;
            }
            set
            {
                _alMaxTime = value;
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
        public TimeSpan? APEndTime
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
        /// Created date: 2008-09-29
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
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public bool Validate()
        {
            bool status = true;

            if(_alMinTime.Equals(TimeSpan.Zero))
                status = false;
            if (status && _alMaxTime.Equals(TimeSpan.Zero))
                status = false;
            if (status && _alMaxTime < _alMinTime)
                status = false;
            if (_alSegment.Equals(TimeSpan.Zero))
                status = false;

            if(status)
            {
                ContainedEntity.ActivityLengthWithSegment = new TimePeriodWithSegment(new TimePeriod(_alMinTime, _alMaxTime), _alSegment);
            }

            ContainedEntity.ExtendWithActivity = (from p in _activities where p.Name.Equals(_currentActivity) select p).First();

            return status;
        }

        #endregion
    }
}
