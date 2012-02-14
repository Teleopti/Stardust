using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Overview
{

    /// <summary>
    /// Used to convert to and from usertimezone and meeting timezone
    /// It checks if the Meeting StartDateTime is valid in the timezone
    /// If not it adds 1 Hour to the time
    /// </summary>
    public class MeetingTimeZoneHelper
    {
        private readonly ICccTimeZoneInfo _userTimeZone;

        public MeetingTimeZoneHelper(ICccTimeZoneInfo userTimeZone)
        {
            _userTimeZone = userTimeZone;
        }

        public ICccTimeZoneInfo UserTimeZone { get { return _userTimeZone; } }

        public DateTime ConvertToUserTimeZone(DateTime dateTime, ICccTimeZoneInfo meetingTimeZone)
        {
            if (meetingTimeZone == null)
                throw new ArgumentNullException("meetingTimeZone");

            var tmp = dateTime.Add(-meetingTimeZone.GetUtcOffset(dateTime));
            tmp = tmp.Add(_userTimeZone.GetUtcOffset(dateTime));
            
            return CheckSoValidInTimeZone(tmp, _userTimeZone);
            
        }

        public static DateTime CheckSoValidInTimeZone(DateTime dateTime, ICccTimeZoneInfo timeZone)
        {
            if (timeZone != null && timeZone.IsInvalidTime(dateTime))
                dateTime = dateTime.AddHours(1);

            return dateTime;
        }

        public DateTime ConvertToMeetingTimeZone(DateTime dateTime, ICccTimeZoneInfo meetingTimeZone)
        {
            if (meetingTimeZone == null)
                throw new ArgumentNullException("meetingTimeZone");

            var tmp = dateTime.Add(-_userTimeZone.GetUtcOffset(dateTime));
            tmp = tmp.Add(meetingTimeZone.GetUtcOffset(dateTime));

            return CheckSoValidInTimeZone(tmp, meetingTimeZone);
        }
    }
}