using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public static class SchedulePartStringVisualizer
    {
        public static string GetToolTipPersonalAssignments(ISchedulePart cell, ICccTimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
        {
            StringBuilder sb = new StringBuilder();

            IList<IPersonAssignment> asses = cell.PersonAssignmentCollection();
            IList<IPersonMeeting> meetings = cell.PersonMeetingCollection();
            if (asses.Count > 0 || meetings.Count > 0)
            {
                foreach (IPersonAssignment pa in asses)
                {
                    foreach (PersonalShift ps in pa.PersonalShiftCollection)
                    {
                        sb.AppendFormat(" - {0}: ", UserTexts.Resources.PersonalShift);
                        foreach (ActivityLayer layer in ps.LayerCollection)
                        {
                            sb.AppendLine();
                            sb.Append("    ");
                            sb.Append(layer.Payload.ConfidentialDescription(pa.Person).Name); 
                            sb.Append(": ");
                            sb.Append(ToLocalStartEndTimeString(layer.Period, timeZoneInfo, cultureInfo));
                        }
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

        private static string ToLocalStartEndTimeString(DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
        {
            const string separator = " - ";
            string start = period.StartDateTimeLocal(timeZoneInfo).ToString("t", cultureInfo);
            string end = period.EndDateTimeLocal(timeZoneInfo).ToString("t", cultureInfo);
            return string.Concat(start, separator, end);
        }
    }
}
