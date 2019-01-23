using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport
{
    public sealed class DimQueueTable
    {
        private readonly TimeZoneInfo _timezone;
        private IFileImportDateTimeParser _parser;

        public DimQueueTable(TimeZoneInfo timeZoneInfo)
        {
            _timezone = timeZoneInfo;
            _parser = new FileImportDateTimeParser();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public  DataTable CreateEmptyDataTable()
        {
            var table = new DataTable {Locale = Thread.CurrentThread.CurrentCulture};

            table.Columns.Add("datetime", typeof(DateTime));
                table.Columns.Add("interval", typeof(string));
                table.Columns.Add("queue_code", typeof(int));
                table.Columns.Add("queue_name", typeof(string));
                table.Columns.Add("offd_direct_call_cnt", typeof(int));
                table.Columns.Add("overflow_in_call_cnt", typeof(int));
                table.Columns.Add("aband_call_cnt", typeof(int));
                table.Columns.Add("overflow_out_call_cnt", typeof(int));
                table.Columns.Add("answ_call_cnt", typeof(int));
                table.Columns.Add("queued_and_answ_call_dur", typeof(int));
                table.Columns.Add("queued_and_aband_call_dur", typeof(int));
                table.Columns.Add("talking_call_dur", typeof(int));
                table.Columns.Add("wrap_up_dur", typeof(int));
                table.Columns.Add("queued_answ_longest_que_dur", typeof(int));
                table.Columns.Add("queued_aband_longest_que_dur", typeof(int));
                table.Columns.Add("avg_avail_member_cnt", typeof(int));
                table.Columns.Add("ans_servicelevel_cnt", typeof(int));
                table.Columns.Add("wait_dur", typeof(int));
                table.Columns.Add("aband_short_call_cnt", typeof(int));
                table.Columns.Add("aband_within_sl_cnt", typeof(int));

                DataColumn[] keys = { table.Columns[0], table.Columns[1], table.Columns[2], table.Columns[3] };
                table.PrimaryKey = keys;

                return table;
        }

        public void Fill(DataTable dt, ICollection<ImportFileDo> collection, IFileImportDateTimeParser parser)
        {
            _parser = parser;
            Fill(dt, collection);
        }

        public void Fill(DataTable dt, ICollection<ImportFileDo> collection)
        {
            int i = 0;
            _parser.TimeZone(_timezone);

            try
            {
                foreach (var importDataObject in collection)
                {
                    i++;

                    if (!_parser.DateTimeIsValid(importDataObject.Date, importDataObject.Time))
                    {
                        throw new FileImportException(
                            string.Format(
                                TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture, UserTexts.Resources.InvalidDateTimeExceptionText,
                                importDataObject.Date + " " + importDataObject.Time, i));
                    }
                    DataRow row = dt.NewRow();
                    DateTime utcDateTime = _parser.UtcDateTime(importDataObject.Date, importDataObject.Time);
                    string utcTimeString = _parser.UtcTime(importDataObject.Date, importDataObject.Time);

                    row["datetime"] = utcDateTime.Date;
                    row["interval"] = utcTimeString;
                    row["queue_code"] = importDataObject.Queue;
                    row["queue_name"] = importDataObject.QueueName;
                    row["offd_direct_call_cnt"] = importDataObject.OfferedDirectCallCount;
                    row["overflow_in_call_cnt"] = importDataObject.OverflowInCallCount;
                    row["aband_call_cnt"] = importDataObject.AbandonCallCount;
                    row["overflow_out_call_cnt"] = importDataObject.OverflowOutcallCount;
                    row["answ_call_cnt"] = importDataObject.AnsweredCallCount;
                    row["queued_and_answ_call_dur"] = importDataObject.QueuedAndAnsweredCallDuration;
                    row["queued_and_aband_call_dur"] = importDataObject.QueuedAndAbandonCallDuration;
                    row["talking_call_dur"] = importDataObject.TalkingCallDuration;
                    row["wrap_up_dur"] = importDataObject.WrapUpDuration;
                    row["queued_answ_longest_que_dur"] = importDataObject.QueuedAnsweredLongestQueueDuration;
                    row["queued_aband_longest_que_dur"] = importDataObject.QueuedAbandonLongestQueueDuration;
                    row["avg_avail_member_cnt"] = importDataObject.AverageAvailMemberCount;
                    row["ans_servicelevel_cnt"] = importDataObject.AnsweredServiceLevelCount;
                    row["wait_dur"] = importDataObject.WaitDuration;
                    row["aband_short_call_cnt"] = importDataObject.AbandonShortCallCount;
                    row["aband_within_sl_cnt"] = importDataObject.AbandonWithinServiceLevelCount;

                    dt.Rows.Add(row);
                }
            }
            catch (ArgumentException)
            {
                throw new FileImportException("Wrong format on line " + i);
            }
            catch (FormatException)
            {
                throw new FileImportException("Wrong format on line " + i);
            }
            catch (ConstraintException)
            {
                throw new FileImportException("Duplicate data on line " + i);
            }
        }
    }
}