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
	    private decimal? _adherenceForPeriod;
	    private decimal? _adherenceForDay;

		public AdherenceDataDto(long localStartTime, long localEndTime, decimal readyTimeMinutes, decimal deviationMinutes, decimal? adherenceForPeriod)
		{
			_localStartTime = localStartTime;
			_localEndTime = localEndTime;
			_readyTimeMinutes = readyTimeMinutes;
			_deviationMinutes = deviationMinutes;
			_adherenceForPeriod = adherenceForPeriod;
		}

		[Obsolete("To better handle future data with no figures, use other constructor with nullable adherence.")]
        public AdherenceDataDto(long localStartTime, long localEndTime, decimal readyTimeMinutes, decimal deviationMinutes, decimal adherence)
        {
            _localStartTime = localStartTime;
            _localEndTime = localEndTime;
            _readyTimeMinutes = readyTimeMinutes;
            _deviationMinutes = deviationMinutes;
            _adherence = adherence;
        }

        [DataMember]
		[Obsolete("To better handle future data with no figures, use AdherenceForPeriod instead")]
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

        [DataMember]
		[Obsolete("Obsolete: To better handle future data with no figures, use AdherenceForDay instead")]
        public decimal DayAdherence
        {
            get { return _dayAdherence; }
            set { _dayAdherence = value; }
        }

		[DataMember]
	    public decimal? AdherenceForPeriod
	    {
			get { return _adherenceForPeriod; }
			set { _adherenceForPeriod = value; }
	    }

		/// <summary>
		/// Gets or sets the day adherence, the summary for the entire day.
		/// </summary>
		/// <value>The day adherence.</value>
		[DataMember]
	    public decimal? AdherenceForDay
	    {
			get { return _adherenceForDay; }
			set { _adherenceForDay = value; }
	    }
    }
}
