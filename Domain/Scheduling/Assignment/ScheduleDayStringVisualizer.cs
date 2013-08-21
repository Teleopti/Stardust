﻿using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public static class ScheduleDayStringVisualizer
    {
		public static string GetToolTipPersonalAssignments(IScheduleDay scheduleDay, TimeZoneInfo timeZoneInfo, IFormatProvider cultureInfo)
        {
            StringBuilder sb = new StringBuilder();

            IList<IPersonAssignment> asses = scheduleDay.PersonAssignmentCollectionDoNotUse();
            IList<IPersonMeeting> meetings = scheduleDay.PersonMeetingCollection();
            if (asses.Count > 0 || meetings.Count > 0)
            {
	            foreach (var pa in asses)
	            {
		            sb.AppendFormat(" - {0}: ", UserTexts.Resources.PersonalShift);
		            foreach (var personalLayer in pa.PersonalLayers())
		            {
			            sb.AppendLine();
			            sb.Append("    ");
			            sb.Append(
				            personalLayer.Payload.ConfidentialDescription(pa.Person, scheduleDay.DateOnlyAsPeriod.DateOnly).Name);
			            sb.Append(": ");
			            sb.Append(ToLocalStartEndTimeString(personalLayer.Period, timeZoneInfo, cultureInfo));
		            }
	            }

	            foreach (IPersonMeeting personMeeting in meetings)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendFormat(" - {0}: ", UserTexts.Resources.Meeting);
                    sb.AppendLine();
                    sb.Append("    ");
                    sb.Append(personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()));
                    sb.Append(": ");
                    sb.Append(ToLocalStartEndTimeString(personMeeting.Period, timeZoneInfo, cultureInfo));

                    if (personMeeting.Optional)
                        sb.AppendFormat(" ({0})", UserTexts.Resources.Optional);
                }
            }

            return sb.ToString();
        }

        public static string ToLocalStartEndTimeString(DateTimePeriod period, TimeZoneInfo timeZoneInfo, IFormatProvider cultureInfo)
        {
            const string separator = " - ";
            string start = period.StartDateTimeLocal(timeZoneInfo).ToString("t", cultureInfo);
            string end = period.EndDateTimeLocal(timeZoneInfo).ToString("t", cultureInfo);
            return string.Concat(start, separator, end);
        }
    }
}
