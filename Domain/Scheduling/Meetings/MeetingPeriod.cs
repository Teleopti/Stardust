using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// Used in Best/Next list for meetings
    /// </summary>
    public class MeetingPeriod
    {
        private DateTimePeriod _period;
        private readonly IList<IMeetingPerson> _meetingPersons;
        private double _value;

        /// <summary>
        /// Constructor
        /// </summary>
        public MeetingPeriod()
        {
            _meetingPersons = new List<IMeetingPerson>();
        }

        /// <summary>
        /// Period
        /// </summary>
        public DateTimePeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }

        /// <summary>
        /// MeetingPersons
        /// </summary>
        public IList<IMeetingPerson> MeetingPersons
        {
            get { return _meetingPersons; }
        }

        /// <summary>
        /// Best value
        /// </summary>
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Add a meetingPerson
        /// </summary>
        /// <param name="meetingPerson"></param>
        public void AddMeetingPerson(IMeetingPerson meetingPerson)
        {
            if(!_meetingPersons.Contains(meetingPerson))
                _meetingPersons.Add(meetingPerson);
        }


        /// <summary>
        /// Add meetingPersons
        /// </summary>
        /// <param name="meetingPersons"></param>
        public void AddMeetingPersons(IList<IMeetingPerson> meetingPersons)
        {
            foreach (IMeetingPerson meetingPerson in meetingPersons)
            {
                AddMeetingPerson(meetingPerson);
            }
        }
    }
}