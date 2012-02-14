using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    //Adapter for wrapping Worktime, readonly for now
    //Uses StartTimeLimitation internally (parselogic)
    public class WorkTimeLimitationAdapter : ILimitation
    {
        private ILimitation _limitation;

        public WorkTimeLimitationAdapter(ILimitation limitation)
        {
            _limitation = limitation;
        }

        public WorkTimeLimitationAdapter(TimePeriod workTime)
        {
            _limitation = new StartTimeLimitation(workTime.StartTime,workTime.EndTime);
        }

        public TimeSpan? StartTime
        {
            get { return _limitation.StartTime; }
            set
            {
                if (value != null) _limitation.StartTime = value;
            }
        }

       
        public TimeSpan? EndTime
        {
            get { return _limitation.EndTime; }
            set
            {
                if (value != null) _limitation.EndTime = value;
            }
        }

       
        public string StartTimeString
        {
            get
            {
                return _limitation.StartTime == TimeSpan.Zero ? string.Empty : _limitation.StartTimeString;
            }
            set
            {
                _limitation.StartTimeString = value;
            }
        }

       
        public string EndTimeString
        {
            get
            {
                return _limitation.EndTime == TimeSpan.Zero ? string.Empty : _limitation.EndTimeString;
            }
            set
            {
                _limitation.EndTimeString = value;   
            }
        }

        public TimeSpan? TimeSpanFromString(string value)
        {
            return _limitation.TimeSpanFromString(value);
        }

        public string StringFromTimeSpan(TimeSpan? value)
        {
            return _limitation.StringFromTimeSpan(value);
        }

        /// <summary>
        /// Determines whether this instance has value.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if StartTime or EndTime has value; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-28
        /// </remarks>
        public bool HasValue()
        {
            throw new System.NotImplementedException();
        }

        public TimePeriod ValidPeriod()
        {
            throw new System.NotImplementedException();
        }

        public TimePeriod WorkTime
        {
            get
            {
                TimeSpan start = (TimeSpan) _limitation.StartTime;
                TimeSpan end = (TimeSpan) _limitation.EndTime;
                return new TimePeriod(start, end);
            }
        }      
    }
}
