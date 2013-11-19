using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class that contains under staffing and serious under staffing dates and hours information.
    /// </summary>
    public class UnderstaffingDetails
    {
        private readonly IList<DateOnly> _seriousUnderstaffingDays = new List<DateOnly>();
        private readonly IList<DateOnly> _understaffingDays = new List<DateOnly>();
        private readonly IList<TimePeriod> _seriousUnderstaffingTimes = new List<TimePeriod>();
        private readonly IList<TimePeriod> _understaffingTimes = new List<TimePeriod>();

        public IEnumerable<DateOnly> SeriousUnderstaffingDays
        {
            get { return _seriousUnderstaffingDays; }
        }

        public IEnumerable<DateOnly> UnderstaffingDays
        {
            get { return _understaffingDays; }
        } 

        public IEnumerable<TimePeriod> SeriousUnderstaffingTimes
        {
            get { return _seriousUnderstaffingTimes; }
        }

        public IEnumerable<TimePeriod> UnderstaffingTimes
        {
            get { return _understaffingTimes; }
        } 

        public void AddSeriousUnderstaffingDay(DateOnly date)
        {
            _seriousUnderstaffingDays.Add(date);
        }

        public void AddUnderstaffingDay(DateOnly date)
        {
            _understaffingDays.Add(date);
        }

        public void AddSeriousUnderstaffingTime(TimePeriod period)
        {
            _seriousUnderstaffingTimes.Add(period);
        }

        public void AddUnderstaffingTime(TimePeriod period)
        {
            _understaffingTimes.Add(period);
        }

        public bool IsNotUnderstaffed()
        {
            return _understaffingDays.Count == 0 &&
                   _understaffingTimes.Count == 0 &&
                   _seriousUnderstaffingDays.Count == 0 &&
                   _seriousUnderstaffingTimes.Count == 0;
        }
    }
}
 