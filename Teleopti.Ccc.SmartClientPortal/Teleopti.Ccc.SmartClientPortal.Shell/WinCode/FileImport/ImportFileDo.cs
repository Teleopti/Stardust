using System;
using System.Globalization;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport
{
    public class ImportFileDo
    {
        public String Interval { set; get; }
        public String Date { set; get; }
        public String Time { set; get; }
        public String Queue { set; get; }
        public String QueueName { set; get; }
        public String OfferedDirectCallCount { set; get; }
        public String OverflowInCallCount { set; get; }
        public String AbandonCallCount { set; get; }
        public String OverflowOutcallCount { set; get; }
        public String AnsweredCallCount { set; get; }
        public String QueuedAndAnsweredCallDuration { set; get; }
        public String QueuedAndAbandonCallDuration { set; get; }
        public String TalkingCallDuration { set; get; }
        public String WrapUpDuration { set; get; }
        public String QueuedAnsweredLongestQueueDuration { set; get; }
        public String QueuedAbandonLongestQueueDuration { set; get; }
        public String AverageAvailMemberCount { set; get; }
        public String AnsweredServiceLevelCount { set; get; }
        public String WaitDuration { set; get; }
        public String AbandonShortCallCount { set; get; }
        public String AbandonWithinServiceLevelCount { set; get; }

        public ImportFileDo(string stream, string separator, int line)
        {
            var strings = stream.Split(Convert.ToChar(separator, CultureInfo.InvariantCulture));

            if (strings.Length != 21)
            {
                throw new FileImportException("Wrong format on line " + line);
            }

            Interval = strings[0];
            Date = strings[1];
            Time = strings[2];
            Queue = strings[3];
            QueueName = strings[4];
            OfferedDirectCallCount = strings[5];
            OverflowInCallCount = strings[6];
            AbandonCallCount = strings[7];
            OverflowOutcallCount = strings[8];
            AnsweredCallCount = strings[9];
            QueuedAndAnsweredCallDuration = strings[10];
            QueuedAndAbandonCallDuration = strings[11];
            TalkingCallDuration = strings[12];
            WrapUpDuration = strings[13];
            QueuedAnsweredLongestQueueDuration = strings[14];
            QueuedAbandonLongestQueueDuration = strings[15];
            AverageAvailMemberCount = strings[16];
            AnsweredServiceLevelCount = strings[17];
            WaitDuration = strings[18];
            AbandonShortCallCount = strings[19];
            AbandonWithinServiceLevelCount = strings[20];
        }

        public static ImportFileDo Create(string stream, string separator, int line)
        {
            return new ImportFileDo(stream, separator, line);
        }
    }
}