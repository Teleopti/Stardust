using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Dto that represents adherence info for a given time period
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AdherenceDataDto
    {
        private long _localStartTime;
        private long _localEndTime;
        private decimal _deviationMinutes;
        private decimal _adherence;
        private decimal _readyTimeMinutes;
        private decimal _dayAdherence;
    	private DateTime _calendarDate;
    	private DateTime _shiftBelongsToDate;

        public AdherenceDataDto(long localStartTime, long localEndTime, decimal readyTimeMinutes, decimal deviationMinutes, decimal adherence, DateTime calendarDate, DateTime shiftBelongsToDate)
        {
            _localStartTime = localStartTime;
            _localEndTime = localEndTime;
            _readyTimeMinutes = readyTimeMinutes;
            _deviationMinutes = deviationMinutes;
            _adherence = adherence;
        	_calendarDate = calendarDate;
        	_shiftBelongsToDate = shiftBelongsToDate;
        }

        [DataMember]
        public decimal Adherence
        {
            get { return _adherence; }
            set { _adherence = value; }
        }

        [DataMember]
        public decimal DeviationMinutes
        {
            get { return _deviationMinutes; }
            set { _deviationMinutes = value; }
        }

        [DataMember]
        public decimal ReadyTimeMinutes
        {
            get { return _readyTimeMinutes; }
            set { _readyTimeMinutes = value; }
        }

        [DataMember]
        public long LocalStartTime
        {
            get { return _localStartTime; }
            set { _localStartTime = value; }
        }

        [DataMember]
        public long LocalEndTime
        {
            get { return _localEndTime; }
            set { _localEndTime = value; }
        }

        /// <summary>
        /// Gets or sets the day adherence, the summery for the entire day.
        /// </summary>
        /// <value>The day adherence.</value>
        [DataMember]
        public decimal DayAdherence
        {
            get { return _dayAdherence; }
            set { _dayAdherence = value; }
        }

		[DataMember]
		public DateTime CalendarDate
		{
			get { return _calendarDate; }
			set { _calendarDate = value; }
		}

		[DataMember]
		public DateTime ShiftBelongsToDate
		{
			get { return _shiftBelongsToDate; }
			set { _shiftBelongsToDate = value; }
		}
    }
}
