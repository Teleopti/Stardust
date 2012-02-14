using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Logging.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class LogEntryDecoder : IEventLogEntryDecoder
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Default constructor which sends in ASCII to the
        /// specific constructor below.
        /// </summary>
        public LogEntryDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Specific EventLogEntryDecoder constructor which takes the Encoding standard
        /// as input parameter upon construction of a new object. The default Encoding standard
        /// id ASCII. 
        /// </summary>
        /// <param name="encoding"></param>
        public LogEntryDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Decode method decodes a stream to an IEventLogEntry.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ILogEntry Decode(Stream source)
        {

            byte[] newline = _encoding.GetBytes(new[] { Consts.Separator });

            IFramerUtility framer = new FramerUtility();
            string logId = _encoding.GetString(framer.NextToken(source, newline));
            string processId = _encoding.GetString(framer.NextToken(source, newline));
            string description = _encoding.GetString(framer.NextToken(source, newline)); 
            string exception = _encoding.GetString(framer.NextToken(source, newline));
            string message = _encoding.GetString(framer.NextToken(source, newline));
            string stackTrace = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            Guid logGuid = new Guid(logId);
            return new LogEntry(logGuid,
                                Int32.Parse(processId, CultureInfo.InvariantCulture),
                                description,
                                exception,
                                message,
                                stackTrace,
                                changedBy,
                                Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// The overloaded Decode method takes a byte array transform it to a stream
        /// and sends it in to the overloaded method above and returns an IEventLogEntry.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public ILogEntry Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }

    }
}