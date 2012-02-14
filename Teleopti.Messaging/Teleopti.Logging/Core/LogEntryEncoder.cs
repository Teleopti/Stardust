using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Logging.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class LogEntryEncoder : IEventLogEntryEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        /// <summary>
        /// The default constructor for the EventLogEntryEncoder, 
        /// it passes the the default encoding standard, ASCII,
        /// into the specific constructor down below.
        /// </summary>
        public LogEntryEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// A specific constructor that takes the 
        /// encoding standard as input parameter.
        /// The default encoding standard is ASCII.
        /// </summary>
        /// <param name="encoding"></param>
        public LogEntryEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Encode method to turn an IEventLogEntry into a byte array.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public byte[] Encode(ILogEntry item)
        {
            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline).");
            
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}{1}", item.LogId, Consts.Separator));
            sb.Append(item.ProcessId);
            sb.Append(Consts.Separator);
            sb.Append(item.Description);
            sb.Append(Consts.Separator);
            sb.Append(item.Exception);
            sb.Append(Consts.Separator);
            sb.Append(item.Message);
            sb.Append(Consts.Separator);
            sb.Append(item.StackTrace);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedBy);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedDateTime.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);

            string EncodedString = sb.ToString();
            if (EncodedString.Length > Consts.MaxWireLength)
                throw new IOException("Encoded length too long, max is 1024 bytes.");

            byte[] buf = _encoding.GetBytes(EncodedString);
            return buf;
        }

    }
}