using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class that contains under staffing and serious under staffing dates and hours information.
    /// </summary>
    public class UnderstaffingDetails
    {
		private readonly HashSet<DateOnly> _seriousUnderstaffingDays = new HashSet<DateOnly>();
		private readonly HashSet<DateOnly> _understaffingDays = new HashSet<DateOnly>();
        private readonly HashSet<TimePeriod> _seriousUnderstaffingTimes = new HashSet<TimePeriod>();
        private readonly HashSet<TimePeriod> _understaffingTimes = new HashSet<TimePeriod>();

        public IEnumerable<DateOnly> SeriousUnderstaffingDays => _seriousUnderstaffingDays;

	    public IEnumerable<DateOnly> UnderstaffingDays => _understaffingDays;

	    public IEnumerable<TimePeriod> SeriousUnderstaffingTimes => _seriousUnderstaffingTimes;

	    public IEnumerable<TimePeriod> UnderstaffingTimes => _understaffingTimes;

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
 