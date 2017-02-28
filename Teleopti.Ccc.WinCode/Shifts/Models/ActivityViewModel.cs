using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Models
{
    public abstract class ActivityViewModel<T> : BaseModel<T>,
                                                 IActivityViewModel<T> where T : IWorkShiftExtender
    {
        private bool _isAutoPosition;
        private TimeSpan _apSegment;

        protected ActivityViewModel(IWorkShiftRuleSet workShiftRuleSet, T containedEntity) 
            : base(workShiftRuleSet, containedEntity)
        {
            ALMinTime = ContainedEntity.ActivityLengthWithSegment.Period.StartTime;
            ALMaxTime = ContainedEntity.ActivityLengthWithSegment.Period.EndTime;
            ALSegment = ContainedEntity.ActivityLengthWithSegment.Segment;
            _apSegment = ContainedEntity.ActivityLengthWithSegment.Segment;

            CurrentActivity = ContainedEntity.ExtendWithActivity;
        }

        public bool IsAutoPosition 
        {
            get
            {
                return _isAutoPosition;
            }
            set
            {
                if (Validate())
                {
	                if (_isAutoPosition != value)
	                {
		                _isAutoPosition = value;
		                OnActivityTypeChanged();
	                }
                }
            }
        }

        public abstract object Count { get; set; }

        public IActivity CurrentActivity { get; set; }

        public TimeSpan ALSegment { get; set; }

        public TimeSpan ALMinTime { get; set; }

        public TimeSpan ALMaxTime { get; set; }

        public virtual TimeSpan APSegment
        {
            get
            {
                return _apSegment;
            }
            set
            {
                _apSegment = value;
            }
        }

        public virtual TimeSpan? APStartTime { get; set; }

        public virtual TimeSpan? APEndTime { get; set; }

        public event EventHandler<ActivityTypeChangedEventArgs> ActivityTypeChanged;

        public bool IsTimeOfDay
        {
            get { return ContainedEntity.GetType() == typeof(ActivityAbsoluteStartExtender); }
        }

        public IWorkShiftExtender WorkShiftExtender
        {
            get { return ContainedEntity; }
        }

        public virtual Type TypeOfClass
        {
            get
            {
                return null;
            }
        }

        protected void OnActivityTypeChanged()
        {
            if(ActivityTypeChanged != null)
                ActivityTypeChanged(this, new ActivityTypeChangedEventArgs(IsAutoPosition ? ActivityType.AutoPosition : ActivityType.AbsolutePosition, this));
        }

        protected void SetAutoPosition(bool autoPosition)
        {
            _isAutoPosition = autoPosition;
        }
    }
}
