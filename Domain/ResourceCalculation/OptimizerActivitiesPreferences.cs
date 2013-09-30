using System;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerActivitiesPreferences : IOptimizerActivitiesPreferences
    {
        private bool _keepShiftCategory;
        private bool _keepStartTime;
        private bool _keepEndTime;
        private TimePeriod? _allowAlterBetween;
        private IList<IActivity> _activities = new List<IActivity>();
        private IList<IActivity> _doNotMoveActivities = new List<IActivity>();
	    public IActivity DoNotAlterLengthOfActivity { get; set; }

	    public bool KeepShiftCategory
        {
            get { return _keepShiftCategory; }
            set { _keepShiftCategory = value; }
        }

        public bool KeepStartTime
        {
            get { return _keepStartTime; }
            set { _keepStartTime = value; }
        }

        public bool KeepEndTime
        {
            get { return _keepEndTime; }
            set { _keepEndTime = value; }
        }

        public TimePeriod? AllowAlterBetween
        {
            get { return _allowAlterBetween; }
            set { _allowAlterBetween = value; }
        }

        public IList<IActivity> Activities
        {
            get { return _activities; }
        }

        public IList<IActivity> DoNotMoveActivities
        {
            get { return _doNotMoveActivities; }
        }

	    

	    public void SetActivities(IList<IActivity> activities)
        {
            _activities = new List<IActivity>(activities);
        }

        public void SetDoNotMoveActivities(IList<IActivity> activities)
        {
            _doNotMoveActivities = activities;
        }

        public DateTimePeriod? UtcPeriodFromDateAndTimePeriod(DateOnly dateOnly, TimeZoneInfo timeZoneInfo)
        {
            if(!AllowAlterBetween.HasValue)
                return null;

            DateTime dt = TimeZoneHelper.ConvertToUtc(dateOnly, timeZoneInfo);
            DateTimePeriod period = new DateTimePeriod(dt.Add(AllowAlterBetween.Value.StartTime), dt.Add(AllowAlterBetween.Value.EndTime));
            
            return period;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }   
}
