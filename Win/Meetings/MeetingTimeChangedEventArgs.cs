using System;

namespace Teleopti.Ccc.Win.Meetings
{

    /// <summary>
    /// Represents a MeetingTimeChangedEventArgs
    /// </summary>
    public class MeetingTimeChangedEventArgs : EventArgs
    {
        public static new MeetingTimeChangedEventArgs Empty
        {
            get
            {
                return new MeetingTimeChangedEventArgs();
            }
        }

        public TimeSpan Start
        {
            get;
            private set;
        }

        public TimeSpan End
        {
            get;
            private set;
        }

        public TimeSpan Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingTimeChangedEventArgs"/> class.
        /// </summary>
        public MeetingTimeChangedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingTimeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="duration">Meeting time duration.</param>
        public MeetingTimeChangedEventArgs(TimeSpan start, TimeSpan end, TimeSpan duration)
        {
            Start = start;
            End = end;
            Duration = duration;
        }
    }
}
