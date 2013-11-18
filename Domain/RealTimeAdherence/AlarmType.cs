using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class AlarmType : VersionedAggregateRootWithBusinessUnit, IAlarmType, IDeleteTag
    {
        private Description _description;
        private Color _displayColor;
        private TimeSpan _thresholdTime;
        private AlarmTypeMode _mode;
        private double _staffingEffect;
        private bool _isDeleted;

        public AlarmType(Description description, Color color, TimeSpan thresholdTime, AlarmTypeMode mode, double staffingEffect)
        {
            _description = description;
            _displayColor = color;
            _thresholdTime = thresholdTime;
            _mode = mode;
            _staffingEffect = staffingEffect;
        }

        protected AlarmType()
        {}

        public virtual  double StaffingEffect
        {
            get { return _staffingEffect; }
            set{ _staffingEffect = value;}
        }

        public virtual TimeSpan ThresholdTime
        {
            get { return _thresholdTime; }
            set
            {
                if (value<TimeSpan.Zero) throw new ArgumentOutOfRangeException("value", "A negative threshold time cannot be used for alarm");
                _thresholdTime = value;
            }
        }

        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set { _displayColor = value; }
        }

		public virtual Description ConfidentialDescription(IPerson assignedPerson, DateOnly assignedDate)
        {
            return Description;
        }

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson, DateOnly assignedDate)
        {
            return DisplayColor;
        }


        public virtual bool InContractTime { get; set; }
        public virtual ITracker Tracker { get; set; }

        public virtual AlarmTypeMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual IPayload UnderlyingPayload
        {
            get { return this; }
        }
    }
}