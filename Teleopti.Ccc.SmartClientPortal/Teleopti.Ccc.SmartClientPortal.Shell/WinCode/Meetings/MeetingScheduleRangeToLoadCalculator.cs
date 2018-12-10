using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    /// <summary>
    /// Calculates the loading period for a person
    /// </summary>
    public class MeetingScheduleRangeToLoadCalculator : ISchedulerRangeToLoadCalculator
    {
        private readonly DateTimePeriod _requestedDateTimePeriod;
        private int _justiceValue = -28; 
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingScheduleRangeToLoadCalculator"/> class.
        /// </summary>
        /// <param name="requestedDateTimePeriod">The requested date time period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-21
        /// </remarks>
        public MeetingScheduleRangeToLoadCalculator(DateTimePeriod requestedDateTimePeriod)
        {
            _requestedDateTimePeriod = requestedDateTimePeriod;
        }

        /// <summary>
        /// Gets the requested period.
        /// </summary>
        /// <value>The requested period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public DateTimePeriod RequestedPeriod
        {
            get
            {
                return _requestedDateTimePeriod;
            }
        }

        /// <summary>
        /// Gets or sets the justice value.
        /// </summary>
        /// <value>The justice value.</value>
        public int JusticeValue
        {
            get { return _justiceValue; }
            set { _justiceValue = value; }
        }

        /// <summary>
        /// Gets the scheduler range to load.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public DateTimePeriod SchedulerRangeToLoad(IPerson person)
        {
            return _requestedDateTimePeriod;
        }
    }
}